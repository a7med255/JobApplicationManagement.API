using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using MediatR;

namespace JobApplicationManagement.Application.Features.Recruiters.Commands;

public record DeleteRecruiterProfileCommand(int Id) : IRequest;

public class DeleteRecruiterProfileCommandHandler : IRequestHandler<DeleteRecruiterProfileCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRecruiterProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteRecruiterProfileCommand request, CancellationToken cancellationToken)
    {
        var recruiter = await _unitOfWork.Recruiters.GetByIdAsync(request.Id);
        if (recruiter is null)
            throw new NotFoundException(nameof(Recruiter), request.Id);

        var isAdmin = _currentUserService.Roles.Contains("Admin");
        if (!isAdmin && recruiter.UserId != _currentUserService.UserId)
            throw new ForbiddenException("You do not have permission to delete this recruiter profile.");

        _unitOfWork.Recruiters.Delete(recruiter);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
