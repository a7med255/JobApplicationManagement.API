using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using JobApplicationManagement.Application.Common.Models;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Application.Features.CandidateCvs.Commands;

public record UploadCandidateCvCommand(
    string DisplayName,
    string OriginalFileName,
    string ContentType,
    byte[] Content,
    bool SetAsDefault) : IRequest<CandidateCvResponseDto>;

public class UploadCandidateCvCommandHandler : IRequestHandler<UploadCandidateCvCommand, CandidateCvResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;
    private readonly FileUploadOptions _options;

    public UploadCandidateCvCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage,
        IOptions<FileUploadOptions> options)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
        _options = options.Value;
    }

    public async Task<CandidateCvResponseDto> Handle(
        UploadCandidateCvCommand request,
        CancellationToken cancellationToken)
    {
        Validate(request, _options.MaximumCvBytes);
        var candidate = await CandidateCvOwner.GetCandidateAsync(
            _unitOfWork, _currentUserService, cancellationToken);
        var existingCvs = await _unitOfWork.CandidateCvs.Query()
            .Where(cv => cv.CandidateId == candidate.Id && !cv.IsDeleted)
            .ToListAsync(cancellationToken);

        var makeDefault = request.SetAsDefault || existingCvs.Count == 0;
        if (makeDefault)
        {
            foreach (var currentDefault in existingCvs.Where(cv => cv.IsDefault))
            {
                currentDefault.IsDefault = false;
            }
        }

        var originalFileName = Path.GetFileName(request.OriginalFileName);
        var storedFile = await _fileStorage.UploadPrivatePdfAsync(
            request.Content,
            originalFileName,
            cancellationToken);
        var cv = new CandidateCv
        {
            CandidateId = candidate.Id,
            DisplayName = request.DisplayName.Trim(),
            OriginalFileName = originalFileName,
            ContentType = "application/pdf",
            Content = null,
            StorageKey = storedFile.StorageKey,
            StorageProvider = storedFile.Provider,
            ResourceType = storedFile.ResourceType,
            DeliveryType = storedFile.DeliveryType,
            ChecksumSha256 = Convert.ToHexString(SHA256.HashData(request.Content)).ToLowerInvariant(),
            FileSize = request.Content.LongLength,
            IsDefault = makeDefault,
            UploadedAt = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.CandidateCvs.AddAsync(cv);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _fileStorage.DeleteAsync(
                storedFile.StorageKey,
                storedFile.ResourceType,
                storedFile.DeliveryType,
                cancellationToken);
            throw;
        }
        return CandidateCvDtoFactory.ToResponse(cv);
    }

    private static void Validate(UploadCandidateCvCommand request, long maximumFileSize)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 200)
            errors[nameof(request.DisplayName)] = ["Display name is required and cannot exceed 200 characters."];
        if (request.Content.Length == 0 || request.Content.LongLength > maximumFileSize)
            errors[nameof(request.Content)] = [$"CV file must be between 1 byte and {maximumFileSize / 1024 / 1024} MB."];
        if (!string.Equals(Path.GetExtension(request.OriginalFileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            errors[nameof(request.OriginalFileName)] = ["Only PDF files are allowed."];
        if (!string.Equals(request.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
            errors[nameof(request.ContentType)] = ["The uploaded file must use the application/pdf content type."];
        if (!HasPdfSignature(request.Content))
            errors[nameof(request.Content)] = ["The uploaded file is not a valid PDF document."];

        if (errors.Count > 0)
            throw new AppValidationException(errors);
    }

    private static bool HasPdfSignature(byte[] content)
    {
        ReadOnlySpan<byte> header = "%PDF-"u8;
        if (content.Length < header.Length || !content.AsSpan(0, header.Length).SequenceEqual(header))
            return false;

        ReadOnlySpan<byte> eof = "%%EOF"u8;
        var tailLength = Math.Min(content.Length, 2048);
        return content.AsSpan(content.Length - tailLength).IndexOf(eof) >= 0;
    }
}
