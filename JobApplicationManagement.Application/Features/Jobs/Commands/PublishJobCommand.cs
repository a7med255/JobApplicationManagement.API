using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record PublishJobCommand(int Id) : IRequest<JobResponseDto>;

public class PublishJobCommandHandler : IRequestHandler<PublishJobCommand, JobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PublishJobCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<JobResponseDto> Handle(PublishJobCommand request, CancellationToken cancellationToken)
    {
        var job = await LoadAsync(request.Id, cancellationToken);
        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
            _unitOfWork, job, _currentUserService, "publish", cancellationToken);
        JobLifecycle.Publish(job, DateTime.UtcNow);
        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();
        return JobDtoFactory.ToResponse(job);
    }

    private async Task<Job> LoadAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.Jobs.Query()
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Job), id);
}
