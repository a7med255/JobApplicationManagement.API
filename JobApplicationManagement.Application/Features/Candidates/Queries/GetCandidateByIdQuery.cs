using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Candidates.Queries;

public record GetCandidateByIdQuery(int Id) : IRequest<CandidateResponseDto>;

public class GetCandidateByIdQueryHandler : IRequestHandler<GetCandidateByIdQuery, CandidateResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCandidateByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CandidateResponseDto> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var candidate = await _unitOfWork.Candidates.Query().AsNoTracking().Include(c => c.Cvs)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (candidate is null)
            throw new NotFoundException(nameof(Candidate), request.Id);

        var isAdmin = _currentUserService.Roles.Contains("Admin");
        if (!isAdmin && candidate.UserId != _currentUserService.UserId)
            throw new ForbiddenException("You do not have permission to view this candidate profile.");

        return CandidateDtoFactory.ToResponse(candidate);
    }
}
