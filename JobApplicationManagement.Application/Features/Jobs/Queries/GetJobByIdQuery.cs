using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Queries;

public record GetJobByIdQuery(int Id) : IRequest<JobResponseDto>;

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetJobByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<JobResponseDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.Query()
            .AsNoTracking()
            .Include(item => item.Company)
            .Include(item => item.JobSkills).ThenInclude(jobSkill => jobSkill.Skill)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.Id);

        var now = DateTime.UtcNow;
        var isPublic = job.Status == JobStatus.Published &&
                       (!job.ApplicationDeadline.HasValue || job.ApplicationDeadline > now);
        if (!isPublic)
        {
            if (!_currentUserService.IsAuthenticated)
                throw new NotFoundException(nameof(Job), request.Id);

            await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
                _unitOfWork, job, _currentUserService, "view", cancellationToken);
        }

        var isSaved = false;
        int? applicationId = null;
        if (_currentUserService.IsAuthenticated && _currentUserService.Roles.Contains("Candidate"))
        {
            var candidateId = await _unitOfWork.Candidates.Query()
                .Where(candidate => candidate.UserId == _currentUserService.UserId)
                .Select(candidate => (int?)candidate.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (candidateId.HasValue)
            {
                isSaved = await _unitOfWork.SavedJobs.Query().AsNoTracking()
                    .AnyAsync(saved => saved.CandidateId == candidateId && saved.JobId == job.Id, cancellationToken);
                applicationId = await _unitOfWork.JobApplications.Query().AsNoTracking()
                    .Where(application => application.CandidateId == candidateId && application.JobId == job.Id)
                    .Select(application => (int?)application.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        return JobDtoFactory.ToResponse(job, isSaved, applicationId);
    }
}
