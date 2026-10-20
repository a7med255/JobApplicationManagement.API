using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.CandidateCvs.Commands;

public record RenameCandidateCvCommand(int Id, string DisplayName) : IRequest<CandidateCvResponseDto>;

public sealed class RenameCandidateCvCommandHandler : IRequestHandler<RenameCandidateCvCommand, CandidateCvResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RenameCandidateCvCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CandidateCvResponseDto> Handle(RenameCandidateCvCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 200)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.DisplayName)] = ["Display name is required and cannot exceed 200 characters."]
            });
        var candidate = await CandidateCvOwner.GetCandidateAsync(_unitOfWork, _currentUser, cancellationToken);
        var cv = await _unitOfWork.CandidateCvs.Query()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CandidateId == candidate.Id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(CandidateCv), request.Id);
        cv.DisplayName = request.DisplayName.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CandidateCvDtoFactory.ToResponse(cv);
    }
}
