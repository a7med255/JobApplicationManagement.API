using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Commands;

public record DeleteJobCommand(int Id) : IRequest;

public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteJobCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(Job), request.Id);
        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
            _unitOfWork, job, _currentUserService, "delete", cancellationToken);

        if (job.Status != JobStatus.Draft)
            throw new ConflictException("Only draft jobs can be permanently deleted. Close and archive published jobs instead.");

        var hasDependencies = await _unitOfWork.JobApplications.Query().AnyAsync(a => a.JobId == job.Id, cancellationToken)
            || await _unitOfWork.SavedJobs.Query().AnyAsync(s => s.JobId == job.Id, cancellationToken);
        if (hasDependencies)
            throw new ConflictException("This job cannot be deleted because it has applications or saved-job references.");

        _unitOfWork.Jobs.Delete(job);
        await _unitOfWork.SaveChangesAsync();
    }
}
