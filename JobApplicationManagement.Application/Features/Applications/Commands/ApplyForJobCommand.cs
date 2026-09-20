using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Commands;

public record ApplyForJobCommand(CreateJobApplicationDto Dto) : IRequest<JobApplicationResponseDto>;

public class ApplyForJobCommandHandler : IRequestHandler<ApplyForJobCommand, JobApplicationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ApplyForJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<JobApplicationResponseDto> Handle(ApplyForJobCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (candidate is null)
            throw new ForbiddenException("Only registered candidates can apply for jobs.");

        var job = await _unitOfWork.Jobs.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == request.Dto.JobId, cancellationToken);

        if (job is null)
            throw new NotFoundException(nameof(Job), request.Dto.JobId);

        if (!job.IsActive)
            throw new ConflictException("Cannot apply to a closed job.");

        var existingApplication = await _unitOfWork.JobApplications.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.CandidateId == candidate.Id && a.JobId == job.Id, cancellationToken);

        if (existingApplication != null)
            throw new ConflictException("You have already applied for this job.");

        var application = _mapper.Map<JobApplication>(request.Dto);
        application.CandidateId = candidate.Id;
        application.JobApplicationStatus = JobApplicationStatus.Applied;
        application.AppliedAt = DateTime.UtcNow;
        application.StatusUpdatedAt = DateTime.UtcNow;

        await _unitOfWork.JobApplications.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<JobApplicationResponseDto>(application);
    }
}
