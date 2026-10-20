using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using JobApplicationManagement.Application.Features.Notifications;

namespace JobApplicationManagement.Application.Features.Applications.Commands;

public record UpdateApplicationStatusCommand(int Id, UpdateJobApplicationDto Dto) : IRequest<JobApplicationResponseDto>;

public sealed class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, JobApplicationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;
    private readonly IEmailQueue _emailQueue;

    public UpdateApplicationStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        IIdentityService identity,
        IEmailQueue emailQueue)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _identity = identity;
        _emailQueue = emailQueue;
    }

    public async Task<JobApplicationResponseDto> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId ?? throw new ForbiddenException("An authenticated user is required.");
        var application = await _unitOfWork.JobApplications.Query()
            .Include(x => x.Job).ThenInclude(x => x.Company)
            .Include(x => x.Candidate)
            .Include(x => x.SubmittedCv)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(JobApplication), request.Id);

        if (!_currentUser.Roles.Contains("Admin"))
        {
            var recruiter = await _unitOfWork.Recruiters.Query().AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);
            if (recruiter is null || application.Job.RecruiterId != recruiter.Id)
                throw new ForbiddenException("You do not have permission to update this application.");
        }

        var target = request.Dto.JobApplicationStatus;
        if (target == JobApplicationStatus.Cancelled)
            throw new ConflictException("Only the candidate can cancel an application.");
        if (!ApplicationStatusTransitions.CanTransition(application.JobApplicationStatus, target))
            throw new ConflictException($"Application cannot move from {application.JobApplicationStatus} to {target}.");

        var oldStatus = application.JobApplicationStatus;
        var now = DateTime.UtcNow;
        application.JobApplicationStatus = target;
        application.StatusUpdatedAt = now;
        await _unitOfWork.ApplicationStatusHistory.AddAsync(new ApplicationStatusHistory
        {
            ApplicationId = application.Id,
            OldStatus = oldStatus,
            NewStatus = target,
            ChangedByUserId = currentUserId,
            ChangedAt = now,
            Note = string.IsNullOrWhiteSpace(request.Dto.Note) ? null : request.Dto.Note.Trim()
        });

        var (notificationType, title, template) = target switch
        {
            JobApplicationStatus.UnderReview => (NotificationType.ApplicationUnderReview, "Application under review", EmailTemplate.ApplicationStatusChanged),
            JobApplicationStatus.Shortlisted => (NotificationType.CandidateShortlisted, "You have been shortlisted", EmailTemplate.CandidateShortlisted),
            JobApplicationStatus.Interview => (NotificationType.InterviewScheduled, "Application moved to interview", EmailTemplate.ApplicationStatusChanged),
            JobApplicationStatus.Accepted => (NotificationType.ApplicationAccepted, "Application accepted", EmailTemplate.ApplicationAccepted),
            JobApplicationStatus.Rejected => (NotificationType.ApplicationRejected, "Application status updated", EmailTemplate.ApplicationRejected),
            _ => (NotificationType.System, "Application status updated", EmailTemplate.ApplicationStatusChanged)
        };
        if (await NotificationPreferenceRules.AllowsInAppAsync(_unitOfWork, application.Candidate.UserId, cancellationToken))
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = application.Candidate.UserId, Type = notificationType, Title = title,
                Message = $"Your application for {application.Job.Title} is now {target}.",
                RelatedEntityId = application.Id, RelatedEntityType = "Application", CreatedAt = now
            });

        var candidateIdentity = await _identity.GetUserByIdAsync(application.Candidate.UserId)
            ?? throw new ConflictException("The candidate account could not be resolved.");
        await _emailQueue.QueueAsync(new EmailMessage(
            candidateIdentity.Email,
            application.Candidate.Name,
            template,
            new Dictionary<string, string>
            {
                ["ApplicationId"] = application.Id.ToString(),
                ["JobTitle"] = application.Job.Title,
                ["Status"] = target.ToString()
            }, application.Candidate.UserId), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<JobApplicationResponseDto>(application);
    }
}
