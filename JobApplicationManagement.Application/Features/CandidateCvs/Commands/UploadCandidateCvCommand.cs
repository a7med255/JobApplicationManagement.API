using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    private const int MaximumFileSize = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UploadCandidateCvCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CandidateCvResponseDto> Handle(
        UploadCandidateCvCommand request,
        CancellationToken cancellationToken)
    {
        Validate(request);
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
        var cv = new CandidateCv
        {
            CandidateId = candidate.Id,
            DisplayName = request.DisplayName.Trim(),
            OriginalFileName = originalFileName,
            ContentType = request.ContentType.Trim().ToLowerInvariant(),
            Content = request.Content,
            FileSize = request.Content.LongLength,
            IsDefault = makeDefault,
            UploadedAt = DateTime.UtcNow
        };

        await _unitOfWork.CandidateCvs.AddAsync(cv);
        await _unitOfWork.SaveChangesAsync();
        return CandidateCvDtoFactory.ToResponse(cv);
    }

    private static void Validate(UploadCandidateCvCommand request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 200)
            errors[nameof(request.DisplayName)] = ["Display name is required and cannot exceed 200 characters."];
        if (request.Content.Length == 0 || request.Content.Length > MaximumFileSize)
            errors[nameof(request.Content)] = ["CV file must be between 1 byte and 5 MB."];
        if (!AllowedExtensions.Contains(Path.GetExtension(request.OriginalFileName)))
            errors[nameof(request.OriginalFileName)] = ["Only PDF, DOC and DOCX files are allowed."];

        if (errors.Count > 0)
            throw new AppValidationException(errors);
    }
}
