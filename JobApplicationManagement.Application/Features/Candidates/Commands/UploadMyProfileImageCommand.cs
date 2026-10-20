using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace JobApplicationManagement.Application.Features.Candidates.Commands;

public sealed record UploadMyProfileImageCommand(ImageUpload Upload) : IRequest<CandidateResponseDto>;

public sealed class UploadMyProfileImageCommandHandler : IRequestHandler<UploadMyProfileImageCommand, CandidateResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;
    private readonly FileUploadOptions _options;
    private readonly ILogger<UploadMyProfileImageCommandHandler> _logger;
    public UploadMyProfileImageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IFileStorageService storage, IOptions<FileUploadOptions> options, ILogger<UploadMyProfileImageCommandHandler> logger)
        => (_unitOfWork, _currentUser, _storage, _options, _logger) = (unitOfWork, currentUser, storage, options.Value, logger);

    public async Task<CandidateResponseDto> Handle(UploadMyProfileImageCommand request, CancellationToken cancellationToken)
    {
        ImageUploadValidation.Validate(request.Upload, _options.MaximumImageBytes);
        var candidate = await _unitOfWork.Candidates.Query().Include(x => x.Cvs)
            .FirstOrDefaultAsync(x => x.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenException("A candidate profile is required.");
        var stored = await _storage.UploadPublicImageAsync(request.Upload.Content, request.Upload.FileName, PublicImageKind.CandidateProfile, cancellationToken);
        var previousKey = candidate.ProfileImagePublicId;
        candidate.ProfileImagePublicId = stored.StorageKey;
        candidate.ProfileImageUrl = stored.SecureUrl;
        try { await _unitOfWork.SaveChangesAsync(cancellationToken); }
        catch
        {
            try { await _storage.DeletePublicImageAsync(stored.StorageKey, cancellationToken); }
            catch (Exception cleanupError) { _logger.LogWarning(cleanupError, "Could not remove orphan candidate image {StorageKey}", stored.StorageKey); }
            throw;
        }
        if (!string.IsNullOrWhiteSpace(previousKey))
        {
            try { await _storage.DeletePublicImageAsync(previousKey, cancellationToken); }
            catch (Exception cleanupError) { _logger.LogWarning(cleanupError, "Could not remove replaced candidate image {StorageKey}", previousKey); }
        }
        return CandidateDtoFactory.ToResponse(candidate);
    }
}
