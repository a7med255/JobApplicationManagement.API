using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.CandidateCvs.Queries;

public record GetMyCandidateCvsQuery : IRequest<IReadOnlyCollection<CandidateCvResponseDto>>;

public class GetMyCandidateCvsQueryHandler : IRequestHandler<GetMyCandidateCvsQuery, IReadOnlyCollection<CandidateCvResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyCandidateCvsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyCollection<CandidateCvResponseDto>> Handle(
        GetMyCandidateCvsQuery request,
        CancellationToken cancellationToken)
    {
        var candidate = await CandidateCvOwner.GetCandidateAsync(
            _unitOfWork, _currentUserService, cancellationToken);
        var cvs = await _unitOfWork.CandidateCvs.Query().AsNoTracking()
            .Where(cv => cv.CandidateId == candidate.Id && !cv.IsDeleted)
            .OrderByDescending(cv => cv.IsDefault)
            .ThenByDescending(cv => cv.UploadedAt)
            .ToListAsync(cancellationToken);
        return cvs.Select(CandidateCvDtoFactory.ToResponse).ToArray();
    }
}
