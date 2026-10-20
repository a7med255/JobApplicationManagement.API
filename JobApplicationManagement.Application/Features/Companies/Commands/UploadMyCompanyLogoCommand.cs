using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Companies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace JobApplicationManagement.Application.Features.Companies.Commands;

public sealed record UploadMyCompanyLogoCommand(ImageUpload Upload) : IRequest<CompanyResponseDto>;

public sealed class UploadMyCompanyLogoCommandHandler : IRequestHandler<UploadMyCompanyLogoCommand, CompanyResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;
    private readonly FileUploadOptions _options;
    private readonly ILogger<UploadMyCompanyLogoCommandHandler> _logger;
    public UploadMyCompanyLogoCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IFileStorageService storage, IOptions<FileUploadOptions> options, ILogger<UploadMyCompanyLogoCommandHandler> logger)
        => (_unitOfWork, _currentUser, _storage, _options, _logger) = (unitOfWork, currentUser, storage, options.Value, logger);

    public async Task<CompanyResponseDto> Handle(UploadMyCompanyLogoCommand request, CancellationToken cancellationToken)
    {
        ImageUploadValidation.Validate(request.Upload, _options.MaximumImageBytes);
        var recruiter = await _unitOfWork.Recruiters.Query().Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenException("A recruiter profile is required.");
        var company = recruiter.Company ?? throw new NotFoundException("Company", recruiter.CompanyId ?? 0);
        var stored = await _storage.UploadPublicImageAsync(request.Upload.Content, request.Upload.FileName, PublicImageKind.CompanyLogo, cancellationToken);
        var previousKey = company.LogoPublicId;
        company.LogoPublicId = stored.StorageKey;
        company.LogoUrl = stored.SecureUrl;
        try { await _unitOfWork.SaveChangesAsync(cancellationToken); }
        catch
        {
            try { await _storage.DeletePublicImageAsync(stored.StorageKey, cancellationToken); }
            catch (Exception cleanupError) { _logger.LogWarning(cleanupError, "Could not remove orphan company logo {StorageKey}", stored.StorageKey); }
            throw;
        }
        if (!string.IsNullOrWhiteSpace(previousKey))
        {
            try { await _storage.DeletePublicImageAsync(previousKey, cancellationToken); }
            catch (Exception cleanupError) { _logger.LogWarning(cleanupError, "Could not remove replaced company logo {StorageKey}", previousKey); }
        }
        return CompanyDtoFactory.ToResponse(company);
    }
}
