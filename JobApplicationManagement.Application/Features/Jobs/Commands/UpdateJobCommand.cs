using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record UpdateJobCommand(int Id, UpdateJobDto JobDto) : IRequest<JobResponseDto>;

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateJobCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<JobResponseDto> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.Query()
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);
        if (job is null)
            throw new NotFoundException(nameof(Job), request.Id);

        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
            _unitOfWork, job, _currentUserService, "update", cancellationToken);
        if (job.Status == JobStatus.Archived)
            throw new ConflictException("An archived job cannot be updated.");

        JobWriteMapper.Apply(request.JobDto, job);
        job.UpdatedAt = DateTime.UtcNow;
        await JobSkillSynchronizer.ReplaceAsync(
            _unitOfWork, job, request.JobDto.RequiredSkills, request.JobDto.PreferredSkills, cancellationToken);

        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();
        return JobDtoFactory.ToResponse(job);
    }
}
