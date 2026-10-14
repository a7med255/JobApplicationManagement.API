using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Application.Features.Jobs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Commands;

public record ApplyForJobCommand(CreateJobApplicationDto Dto) : IRequest<JobApplicationResponseDto>;

public class ApplyForJobCommandHandler : IRequestHandler<ApplyForJobCommand, JobApplicationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ApplyForJobCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<JobApplicationResponseDto> Handle(
        ApplyForJobCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await _unitOfWork.Candidates.Query()
            .FirstOrDefaultAsync(item => item.UserId == _currentUserService.UserId, cancellationToken)
            ?? throw new ForbiddenException("Only registered candidates can apply for jobs.");
        var job = await _unitOfWork.Jobs.Query().AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Dto.JobId, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.Dto.JobId);

        if (!JobLifecycle.CanAcceptApplications(job, DateTime.UtcNow))
            throw new ConflictException("This job is not accepting applications.");

        var alreadyApplied = await _unitOfWork.JobApplications.Query().AsNoTracking()
            .AnyAsync(application => application.CandidateId == candidate.Id && application.JobId == job.Id,
                cancellationToken);
        if (alreadyApplied)
            throw new ConflictException("You have already applied for this job.");

        var cvQuery = _unitOfWork.CandidateCvs.Query()
            .Where(cv => cv.CandidateId == candidate.Id && !cv.IsDeleted);
        var submittedCv = request.Dto.CandidateCvId.HasValue
            ? await cvQuery.FirstOrDefaultAsync(cv => cv.Id == request.Dto.CandidateCvId.Value, cancellationToken)
            : await cvQuery.FirstOrDefaultAsync(cv => cv.IsDefault, cancellationToken);
        if (submittedCv is null)
            throw new ConflictException(request.Dto.CandidateCvId.HasValue
                ? "The selected CV was not found."
                : "Upload a CV or select a default CV before applying.");

        var now = DateTime.UtcNow;
        var application = new JobApplication
        {
            CandidateId = candidate.Id,
            Candidate = candidate,
            JobId = job.Id,
            Job = job,
            SubmittedCvId = submittedCv.Id,
            SubmittedCv = submittedCv,
            JobApplicationStatus = JobApplicationStatus.Applied,
            AppliedAt = now,
            StatusUpdatedAt = now
        };

        await _unitOfWork.JobApplications.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();
        return new JobApplicationResponseDto
        {
            Id = application.Id,
            CandidateId = candidate.Id,
            CandidateName = candidate.Name,
            JobId = job.Id,
            JobTitle = job.Title,
            SubmittedCvId = submittedCv.Id,
            SubmittedCvDisplayName = submittedCv.DisplayName,
            SubmittedCvFileName = submittedCv.OriginalFileName,
            JobApplicationStatus = application.JobApplicationStatus,
            AppliedAt = now,
            StatusUpdatedAt = now
        };
    }
}
