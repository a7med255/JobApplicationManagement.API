using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.CandidateCvs.Commands;

public record DeleteCandidateCvCommand(int Id) : IRequest;

public class DeleteCandidateCvCommandHandler : IRequestHandler<DeleteCandidateCvCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public DeleteCandidateCvCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task Handle(DeleteCandidateCvCommand request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateCvOwner.GetCandidateAsync(
            _unitOfWork, _currentUserService, cancellationToken);
        var cv = await _unitOfWork.CandidateCvs.Query()
            .FirstOrDefaultAsync(item => item.Id == request.Id && item.CandidateId == candidate.Id && !item.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException(nameof(CandidateCv), request.Id);

        var wasDefault = cv.IsDefault;
        cv.IsDeleted = true;
        cv.DeletedAt = DateTime.UtcNow;
        cv.IsDefault = false;
        if (wasDefault)
        {
            var replacement = await _unitOfWork.CandidateCvs.Query()
                .Where(item => item.CandidateId == candidate.Id && !item.IsDeleted && item.Id != cv.Id)
                .OrderByDescending(item => item.UploadedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (replacement is not null)
                replacement.IsDefault = true;
        }

        var isReferenced = await _unitOfWork.JobApplications.Query().AsNoTracking()
            .AnyAsync(item => item.SubmittedCvId == cv.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!isReferenced && !string.IsNullOrWhiteSpace(cv.StorageKey))
            await _fileStorage.DeleteAsync(cv.StorageKey, cv.ResourceType, cv.DeliveryType, cancellationToken);
    }
}
