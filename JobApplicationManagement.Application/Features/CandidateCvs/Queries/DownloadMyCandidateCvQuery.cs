using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.CandidateCvs.Queries;

public record DownloadMyCandidateCvQuery(int Id) : IRequest<FileDownloadDto>;

public class DownloadMyCandidateCvQueryHandler : IRequestHandler<DownloadMyCandidateCvQuery, FileDownloadDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DownloadMyCandidateCvQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<FileDownloadDto> Handle(DownloadMyCandidateCvQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateCvOwner.GetCandidateAsync(
            _unitOfWork, _currentUserService, cancellationToken);
        var cv = await _unitOfWork.CandidateCvs.Query().AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id && item.CandidateId == candidate.Id && !item.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException(nameof(CandidateCv), request.Id);
        return new FileDownloadDto(cv.Content, cv.ContentType, cv.OriginalFileName);
    }
}
