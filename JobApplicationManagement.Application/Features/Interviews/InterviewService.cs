using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;
using JobApplicationManagement.Application.Features.Notifications;

namespace JobApplicationManagement.Application.Features.Interviews;

public sealed class InterviewService : IInterviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;
    private readonly IEmailQueue _emailQueue;

    public InterviewService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identity,
        IEmailQueue emailQueue)
        => (_unitOfWork, _currentUser, _identity, _emailQueue) = (unitOfWork, currentUser, identity, emailQueue);

    public async Task<IReadOnlyCollection<InterviewDto>> GetForApplicationAsync(int applicationId, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(applicationId, cancellationToken);
        await EnsureCanReadAsync(application, cancellationToken);
        var interviews = await _unitOfWork.Interviews.Query().AsNoTracking()
            .Where(x => x.ApplicationId == applicationId).OrderByDescending(x => x.ScheduledAt)
            .ToListAsync(cancellationToken);
        return interviews.Select(ToDto).ToArray();
    }

    public async Task<InterviewDto> ScheduleAsync(ScheduleInterviewDto request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new ForbiddenException("An authenticated user is required.");
        Validate(request.ScheduledAt, request.DurationMinutes, request.InterviewType, request.MeetingLink, request.Location, request.Notes);
        var application = await LoadApplicationAsync(request.ApplicationId, cancellationToken);
        await EnsureCanManageAsync(application, cancellationToken);
        if (application.JobApplicationStatus is not (JobApplicationStatus.UnderReview or JobApplicationStatus.Shortlisted))
            throw new ConflictException("Interviews can be scheduled only for applications under review or shortlisted.");

        var now = DateTime.UtcNow;
        var interview = new Interview
        {
            ApplicationId = application.Id,
            ScheduledAt = request.ScheduledAt.ToUniversalTime(),
            DurationMinutes = request.DurationMinutes,
            InterviewType = request.InterviewType,
            MeetingLink = Normalize(request.MeetingLink),
            Location = Normalize(request.Location),
            Notes = Normalize(request.Notes),
            Status = InterviewStatus.Scheduled,
            ScheduledByUserId = currentUserId,
            CreatedAt = now,
            UpdatedAt = now
        };
        var oldStatus = application.JobApplicationStatus;
        application.JobApplicationStatus = JobApplicationStatus.Interview;
        application.StatusUpdatedAt = now;
        await _unitOfWork.Interviews.AddAsync(interview);
        await _unitOfWork.ApplicationStatusHistory.AddAsync(new ApplicationStatusHistory
        {
            ApplicationId = application.Id, OldStatus = oldStatus, NewStatus = JobApplicationStatus.Interview,
            ChangedByUserId = currentUserId, ChangedAt = now, Note = "Interview scheduled"
        });
        await NotifyCandidateAsync(application, NotificationType.InterviewScheduled, EmailTemplate.InterviewScheduled,
            "Interview scheduled", interview.ScheduledAt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(interview);
    }

    public async Task<InterviewDto> RescheduleAsync(int id, RescheduleInterviewDto request, CancellationToken cancellationToken)
    {
        Validate(request.ScheduledAt, request.DurationMinutes, request.InterviewType, request.MeetingLink, request.Location, request.Notes);
        var interview = await LoadInterviewAsync(id, cancellationToken);
        await EnsureCanManageAsync(interview.Application, cancellationToken);
        if (interview.Status == InterviewStatus.Cancelled)
            throw new ConflictException("A cancelled interview cannot be rescheduled.");
        interview.ScheduledAt = request.ScheduledAt.ToUniversalTime();
        interview.DurationMinutes = request.DurationMinutes;
        interview.InterviewType = request.InterviewType;
        interview.MeetingLink = Normalize(request.MeetingLink);
        interview.Location = Normalize(request.Location);
        interview.Notes = Normalize(request.Notes);
        interview.UpdatedAt = DateTime.UtcNow;
        await NotifyCandidateAsync(interview.Application, NotificationType.InterviewRescheduled, EmailTemplate.InterviewRescheduled,
            "Interview rescheduled", interview.ScheduledAt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(interview);
    }

    public async Task CancelAsync(int id, CancellationToken cancellationToken)
    {
        var interview = await LoadInterviewAsync(id, cancellationToken);
        await EnsureCanManageAsync(interview.Application, cancellationToken);
        if (interview.Status == InterviewStatus.Cancelled) return;
        interview.Status = InterviewStatus.Cancelled;
        interview.UpdatedAt = DateTime.UtcNow;
        await NotifyCandidateAsync(interview.Application, NotificationType.InterviewCancelled, EmailTemplate.InterviewCancelled,
            "Interview cancelled", interview.ScheduledAt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<JobApplication> LoadApplicationAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.JobApplications.Query()
            .Include(x => x.Candidate)
            .Include(x => x.Job).ThenInclude(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(JobApplication), id);

    private async Task<Interview> LoadInterviewAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.Interviews.Query()
            .Include(x => x.Application).ThenInclude(x => x.Candidate)
            .Include(x => x.Application).ThenInclude(x => x.Job).ThenInclude(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Interview), id);

    private async Task EnsureCanReadAsync(JobApplication application, CancellationToken cancellationToken)
    {
        if (_currentUser.Roles.Contains("Admin")) return;
        if (_currentUser.Roles.Contains("Candidate") && application.Candidate.UserId == _currentUser.UserId) return;
        await EnsureCanManageAsync(application, cancellationToken);
    }

    private async Task EnsureCanManageAsync(JobApplication application, CancellationToken cancellationToken)
    {
        if (_currentUser.Roles.Contains("Admin")) return;
        if (!_currentUser.Roles.Contains("Recruiter"))
            throw new ForbiddenException("You do not have permission to manage this interview.");
        var ownsJob = await _unitOfWork.Recruiters.Query().AsNoTracking()
            .AnyAsync(x => x.UserId == (_currentUser.UserId ?? string.Empty) && x.Id == application.Job.RecruiterId, cancellationToken);
        if (!ownsJob) throw new ForbiddenException("You do not have permission to manage this interview.");
    }

    private async Task NotifyCandidateAsync(
        JobApplication application,
        NotificationType type,
        EmailTemplate template,
        string title,
        DateTime scheduledAt,
        CancellationToken cancellationToken)
    {
        if (await NotificationPreferenceRules.AllowsInAppAsync(_unitOfWork, application.Candidate.UserId, cancellationToken))
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = application.Candidate.UserId, Type = type, Title = title,
                Message = $"{title} for {application.Job.Title} on {scheduledAt:u}.",
                RelatedEntityId = application.Id, RelatedEntityType = "Application", CreatedAt = DateTime.UtcNow
            });
        var account = await _identity.GetUserByIdAsync(application.Candidate.UserId)
            ?? throw new ConflictException("The candidate account could not be resolved.");
        await _emailQueue.QueueAsync(new EmailMessage(
            account.Email, application.Candidate.Name, template,
            new Dictionary<string, string>
            {
                ["ApplicationId"] = application.Id.ToString(),
                ["JobTitle"] = application.Job.Title,
                ["ScheduledAt"] = scheduledAt.ToString("u")
            }, application.Candidate.UserId), cancellationToken);
    }

    private static void Validate(DateTime scheduledAt, int duration, InterviewType type, string? meetingLink, string? location, string? notes)
    {
        var errors = new Dictionary<string, string[]>();
        if (scheduledAt.ToUniversalTime() <= DateTime.UtcNow.AddMinutes(5)) errors["ScheduledAt"] = ["Interview must be scheduled in the future."];
        if (duration is < 15 or > 480) errors["DurationMinutes"] = ["Duration must be between 15 and 480 minutes."];
        if (!Enum.IsDefined(type)) errors["InterviewType"] = ["Interview type is invalid."];
        if (type == InterviewType.Online && !Uri.TryCreate(meetingLink, UriKind.Absolute, out _)) errors["MeetingLink"] = ["A valid meeting link is required for online interviews."];
        if (type == InterviewType.OnSite && string.IsNullOrWhiteSpace(location)) errors["Location"] = ["A location is required for on-site interviews."];
        if (notes?.Length > 2000) errors["Notes"] = ["Notes cannot exceed 2000 characters."];
        if (errors.Count > 0) throw new AppValidationException(errors);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static InterviewDto ToDto(Interview x) => new()
    {
        Id = x.Id, ApplicationId = x.ApplicationId, ScheduledAt = x.ScheduledAt,
        DurationMinutes = x.DurationMinutes, InterviewType = x.InterviewType,
        MeetingLink = x.MeetingLink, Location = x.Location, Notes = x.Notes,
        Status = x.Status, CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };
}
