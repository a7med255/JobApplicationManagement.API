using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using MediatR;

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
        var job = await _unitOfWork.Jobs.GetByIdAsync(request.Id);
        if (job is null)
            throw new NotFoundException(nameof(Job), request.Id);

        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(_unitOfWork, job, _currentUserService, "delete");

        _unitOfWork.Jobs.Delete(job);
        await _unitOfWork.SaveChangesAsync();
    }
}
