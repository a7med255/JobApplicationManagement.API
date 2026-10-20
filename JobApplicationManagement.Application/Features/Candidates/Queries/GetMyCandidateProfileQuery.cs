using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Candidates.Queries;

public record GetMyCandidateProfileQuery : IRequest<CandidateResponseDto>;

public class GetMyCandidateProfileQueryHandler : IRequestHandler<GetMyCandidateProfileQuery, CandidateResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyCandidateProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CandidateResponseDto> Handle(GetMyCandidateProfileQuery request, CancellationToken cancellationToken)
    {
        var candidate = await _unitOfWork.Candidates.Query().AsNoTracking().Include(c => c.Cvs)
            .FirstOrDefaultAsync(c => c.UserId == _currentUserService.UserId, cancellationToken);

        return candidate is null
            ? throw new NotFoundException(nameof(Candidate), _currentUserService.UserId ?? "current user")
            : CandidateDtoFactory.ToResponse(candidate);
    }
}
