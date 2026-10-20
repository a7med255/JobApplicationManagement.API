namespace JobApplicationManagement.Application.Common.Interfaces;

public sealed record StoredFile(
    string StorageKey,
    string Provider,
    string ResourceType,
    string DeliveryType,
    long Bytes);

public enum PublicImageKind { CandidateProfile, CompanyLogo }

public sealed record StoredImage(string StorageKey, string SecureUrl, long Bytes);

public interface IFileStorageService
{
    Task<StoredFile> UploadPrivatePdfAsync(
        byte[] content,
        string safeFileName,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadPrivateAsync(
        string storageKey,
        string resourceType,
        string deliveryType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        string resourceType,
        string deliveryType,
        CancellationToken cancellationToken = default);

    Task<StoredImage> UploadPublicImageAsync(
        byte[] content,
        string safeFileName,
        PublicImageKind kind,
        CancellationToken cancellationToken = default);

    Task DeletePublicImageAsync(string storageKey, CancellationToken cancellationToken = default);
}
