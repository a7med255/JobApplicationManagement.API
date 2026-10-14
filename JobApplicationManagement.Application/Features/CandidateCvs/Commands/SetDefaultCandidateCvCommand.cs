using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.CandidateCvs.Commands;

public record SetDefaultCandidateCvCommand(int Id) : IRequest<CandidateCvResponseDto>;

public class SetDefaultCandidateCvCommandHandler : IRequestHandler<SetDefaultCandidateCvCommand, CandidateCvResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SetDefaultCandidateCvCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CandidateCvResponseDto> Handle(
        SetDefaultCandidateCvCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await CandidateCvOwner.GetCandidateAsync(
            _unitOfWork, _currentUserService, cancellationToken);
        var cvs = await _unitOfWork.CandidateCvs.Query()
            .Where(cv => cv.CandidateId == candidate.Id && !cv.IsDeleted)
            .ToListAsync(cancellationToken);
        var selected = cvs.FirstOrDefault(cv => cv.Id == request.Id)
            ?? throw new NotFoundException(nameof(CandidateCv), request.Id);

        foreach (var cv in cvs)
            cv.IsDefault = cv.Id == selected.Id;

        await _unitOfWork.SaveChangesAsync();
        return CandidateCvDtoFactory.ToResponse(selected);
    }
}
