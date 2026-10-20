using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using MediatR;

namespace JobApplicationManagement.Application.Features.Candidates.Commands;

public record DeleteCandidateProfileCommand(int Id) : IRequest;

public class DeleteCandidateProfileCommandHandler : IRequestHandler<DeleteCandidateProfileCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCandidateProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteCandidateProfileCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _unitOfWork.Candidates.GetByIdAsync(request.Id);
        if (candidate is null)
            throw new NotFoundException(nameof(Candidate), request.Id);

        var isAdmin = _currentUserService.Roles.Contains("Admin");
        if (!isAdmin && candidate.UserId != _currentUserService.UserId)
            throw new ForbiddenException("You do not have permission to delete this candidate profile.");

        _unitOfWork.Candidates.Delete(candidate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
