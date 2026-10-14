using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record ReopenJobCommand(int Id) : IRequest<JobResponseDto>;

public class ReopenJobCommandHandler : IRequestHandler<ReopenJobCommand, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReopenJobCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<JobResponseDto> Handle(ReopenJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.Query()
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.Id);
        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
            _unitOfWork, job, _currentUserService, "reopen", cancellationToken);
        if (job.Status is not (JobStatus.Closed or JobStatus.Expired))
            throw new ConflictException("Only a closed or expired job can be reopened.");

        JobLifecycle.Publish(job, DateTime.UtcNow);
        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();
        return JobDtoFactory.ToResponse(job);
    }
}
