using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using JobApplicationManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace JobApplicationManagement.Infrastructure.Storage;

public sealed class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;
    private readonly HttpClient _httpClient;

    public CloudinaryFileStorageService(
        IOptions<CloudinaryOptions> options,
        HttpClient httpClient)
    {
        _options = options.Value;
        _cloudinary = new Cloudinary(new Account(
            _options.CloudName,
            _options.ApiKey,
            _options.ApiSecret))
        {
            Api = { Secure = true }
        };
        _httpClient = httpClient;
    }

    public async Task<StoredFile> UploadPrivatePdfAsync(
        byte[] content,
        string safeFileName,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var identifier = $"{_options.CvFolder}/{Guid.NewGuid():N}.pdf";
        await using var stream = new MemoryStream(content, writable: false);
        var upload = await _cloudinary.UploadAsync(new RawUploadParams
        {
            File = new FileDescription(safeFileName, stream),
            PublicId = identifier,
            Type = "authenticated",
            Overwrite = false
        });

        if (upload.Error is not null || string.IsNullOrWhiteSpace(upload.PublicId))
            throw new InvalidOperationException(upload.Error?.Message ?? "Cloudinary did not return a storage identifier.");

        return new StoredFile(upload.PublicId, "Cloudinary", "raw", "authenticated", upload.Bytes);
    }

    public async Task<byte[]> DownloadPrivateAsync(
        string storageKey,
        string resourceType,
        string deliveryType,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var url = _cloudinary.Api.UrlImgUp
            .ResourceType(resourceType)
            .Type(deliveryType)
            .Signed(true)
            .BuildUrl(storageKey);
        return await _httpClient.GetByteArrayAsync(url, cancellationToken);
    }

    public async Task DeleteAsync(
        string storageKey,
        string resourceType,
        string deliveryType,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var result = await _cloudinary.DestroyAsync(new DeletionParams(storageKey)
        {
            ResourceType = resourceType == "raw" ? ResourceType.Raw : ResourceType.Image,
            Type = deliveryType,
            Invalidate = true
        });
        if (result.Error is not null)
            throw new InvalidOperationException(result.Error.Message);
    }

    public async Task<StoredImage> UploadPublicImageAsync(
        byte[] content,
        string safeFileName,
        PublicImageKind kind,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var category = kind == PublicImageKind.CandidateProfile ? "profiles" : "companies";
        var identifier = $"{_options.ImageFolder}/{category}/{Guid.NewGuid():N}";
        await using var stream = new MemoryStream(content, writable: false);
        var upload = await _cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(safeFileName, stream),
            PublicId = identifier,
            Type = "upload",
            Overwrite = false,
            Transformation = new Transformation().Width(800).Height(800).Crop("limit").Quality("auto").FetchFormat("auto")
        }, cancellationToken);

        if (upload.Error is not null || string.IsNullOrWhiteSpace(upload.PublicId) || upload.SecureUrl is null)
            throw new InvalidOperationException(upload.Error?.Message ?? "Cloudinary did not return image metadata.");

        return new StoredImage(upload.PublicId, upload.SecureUrl.AbsoluteUri, upload.Bytes);
    }

    public async Task DeletePublicImageAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var result = await _cloudinary.DestroyAsync(new DeletionParams(storageKey)
        {
            ResourceType = ResourceType.Image,
            Type = "upload",
            Invalidate = true
        });
        if (result.Error is not null)
            throw new InvalidOperationException(result.Error.Message);
    }

    private void EnsureConfigured()
    {
        if (!_options.IsConfigured)
            throw new InvalidOperationException("Cloudinary is not configured. Provide CloudName, ApiKey, and ApiSecret in Cloudinary settings or environment variables.");
    }
}
