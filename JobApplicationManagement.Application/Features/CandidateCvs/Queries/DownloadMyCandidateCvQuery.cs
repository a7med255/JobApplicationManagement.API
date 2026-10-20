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
    private readonly IFileStorageService _fileStorage;

    public DownloadMyCandidateCvQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<FileDownloadDto> Handle(DownloadMyCandidateCvQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateCvOwner.GetCandidateAsync(
            _unitOfWork, _currentUserService, cancellationToken);
        var cv = await _unitOfWork.CandidateCvs.Query().AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id && item.CandidateId == candidate.Id && !item.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException(nameof(CandidateCv), request.Id);
        var content = cv.Content;
        if (content is null && !string.IsNullOrWhiteSpace(cv.StorageKey))
            content = await _fileStorage.DownloadPrivateAsync(
                cv.StorageKey, cv.ResourceType, cv.DeliveryType, cancellationToken);
        if (content is null)
            throw new NotFoundException(nameof(CandidateCv), request.Id);
        return new FileDownloadDto(content, cv.ContentType, cv.OriginalFileName);
    }
}
