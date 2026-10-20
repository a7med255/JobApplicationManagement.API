using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record CreateJobCommand(CreateJobDto JobDto) : IRequest<JobResponseDto>;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateJobCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<JobResponseDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
            throw new UnauthorizedException("Authentication is required to create a job.");

        var recruiter = await _unitOfWork.Recruiters.Query()
            .Include(r => r.Company)
            .FirstOrDefaultAsync(r => r.UserId == _currentUserService.UserId, cancellationToken);

        if (recruiter is null)
            throw new ForbiddenException("Only registered recruiters can create jobs.");
        if (recruiter.Company is null)
            throw new ConflictException("Complete your company profile before creating a job.");

        var now = DateTime.UtcNow;
        var job = new Job
        {
            RecruiterId = recruiter.Id,
            Recruiter = recruiter,
            CompanyId = recruiter.Company.Id,
            Company = recruiter.Company,
            Status = JobStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };
        JobWriteMapper.Apply(request.JobDto, job);
        await JobSkillSynchronizer.ReplaceAsync(
            _unitOfWork, job, request.JobDto.RequiredSkills, request.JobDto.PreferredSkills, cancellationToken);

        if (request.JobDto.PublishImmediately)
            JobLifecycle.Publish(job, now);

        await _unitOfWork.Jobs.AddAsync(job);
        await _unitOfWork.SaveChangesAsync();
        return JobDtoFactory.ToResponse(job);
    }
}
