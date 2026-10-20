using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Application.Features.Jobs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using JobApplicationManagement.Application.Features.Notifications;

namespace JobApplicationManagement.Application.Features.Applications.Commands;

public record ApplyForJobCommand(CreateJobApplicationDto Dto) : IRequest<JobApplicationResponseDto>;

public sealed class ApplyForJobCommandHandler : IRequestHandler<ApplyForJobCommand, JobApplicationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;
    private readonly IEmailQueue _emailQueue;

    public ApplyForJobCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identity,
        IEmailQueue emailQueue)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identity = identity;
        _emailQueue = emailQueue;
    }

    public async Task<JobApplicationResponseDto> Handle(ApplyForJobCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _unitOfWork.Candidates.Query()
            .FirstOrDefaultAsync(x => x.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenException("Only registered candidates can apply for jobs.");
        var job = await _unitOfWork.Jobs.Query().AsNoTracking()
            .Include(x => x.Company)
            .Include(x => x.Recruiter)
            .FirstOrDefaultAsync(x => x.Id == request.Dto.JobId, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.Dto.JobId);

        if (!JobLifecycle.CanAcceptApplications(job, DateTime.UtcNow))
            throw new ConflictException("This job is not accepting applications.");
        if (await _unitOfWork.JobApplications.Query().AsNoTracking()
            .AnyAsync(x => x.CandidateId == candidate.Id && x.JobId == job.Id, cancellationToken))
            throw new ConflictException("You have already applied for this job.");

        var cvQuery = _unitOfWork.CandidateCvs.Query()
            .Where(x => x.CandidateId == candidate.Id && !x.IsDeleted);
        var cv = request.Dto.CandidateCvId.HasValue
            ? await cvQuery.FirstOrDefaultAsync(x => x.Id == request.Dto.CandidateCvId.Value, cancellationToken)
            : await cvQuery.FirstOrDefaultAsync(x => x.IsDefault, cancellationToken);
        if (cv is null)
            throw new ConflictException(request.Dto.CandidateCvId.HasValue
                ? "The selected CV was not found."
                : "Upload a CV or select a default CV before applying.");

        var candidateIdentity = await _identity.GetUserByIdAsync(candidate.UserId)
            ?? throw new ForbiddenException("The candidate account could not be resolved.");
        var recruiterIdentity = await _identity.GetUserByIdAsync(job.Recruiter.UserId)
            ?? throw new ConflictException("The job recruiter account could not be resolved.");
        var now = DateTime.UtcNow;
        var application = new JobApplication
        {
            CandidateId = candidate.Id,
            Candidate = candidate,
            JobId = job.Id,
            Job = job,
            SubmittedCvId = cv.Id,
            SubmittedCv = cv,
            CoverLetter = string.IsNullOrWhiteSpace(request.Dto.CoverLetter) ? null : request.Dto.CoverLetter.Trim(),
            JobApplicationStatus = JobApplicationStatus.Applied,
            AppliedAt = now,
            StatusUpdatedAt = now
        };

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _unitOfWork.JobApplications.AddAsync(application);
            await _unitOfWork.SaveChangesAsync(ct);

            await _unitOfWork.ApplicationStatusHistory.AddAsync(new ApplicationStatusHistory
            {
                ApplicationId = application.Id,
                NewStatus = JobApplicationStatus.Applied,
                ChangedByUserId = candidate.UserId,
                ChangedAt = now,
                Note = "Application submitted"
            });
            if (await NotificationPreferenceRules.AllowsInAppAsync(_unitOfWork, candidate.UserId, ct))
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserId = candidate.UserId, Type = NotificationType.ApplicationSubmitted,
                    Title = "Application submitted", Message = $"Your application for {job.Title} at {job.Company.Name} was submitted.",
                    RelatedEntityId = application.Id, RelatedEntityType = "Application", CreatedAt = now
                });
            if (await NotificationPreferenceRules.AllowsInAppAsync(_unitOfWork, job.Recruiter.UserId, ct))
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserId = job.Recruiter.UserId, Type = NotificationType.NewApplication,
                    Title = "New candidate application", Message = $"{candidate.Name} applied for {job.Title}.",
                    RelatedEntityId = application.Id, RelatedEntityType = "Application", CreatedAt = now
                });

            var data = new Dictionary<string, string>
            {
                ["ApplicationId"] = application.Id.ToString(),
                ["JobTitle"] = job.Title,
                ["CompanyName"] = job.Company.Name,
                ["CandidateName"] = candidate.Name,
                ["ApplicationDate"] = now.ToString("yyyy-MM-dd HH:mm 'UTC'")
            };
            await _emailQueue.QueueAsync(new EmailMessage(
                candidateIdentity.Email, candidate.Name, EmailTemplate.ApplicationSubmitted, data, candidate.UserId), ct);
            await _emailQueue.QueueAsync(new EmailMessage(
                recruiterIdentity.Email, job.Recruiter.Name, EmailTemplate.NewCandidateApplication, data, job.Recruiter.UserId), ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return new JobApplicationResponseDto
        {
            Id = application.Id,
            CandidateId = candidate.Id,
            CandidateName = candidate.Name,
            JobId = job.Id,
            JobTitle = job.Title,
            CompanyName = job.Company.Name,
            SubmittedCvId = cv.Id,
            SubmittedCvDisplayName = cv.DisplayName,
            SubmittedCvFileName = cv.OriginalFileName,
            CoverLetter = application.CoverLetter,
            JobApplicationStatus = application.JobApplicationStatus,
            AppliedAt = now,
            StatusUpdatedAt = now
        };
    }
}
