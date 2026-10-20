namespace JobApplicationManagement.Domain.Entities;

public class CandidateCv
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
    public string DisplayName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    /// <summary>Legacy database content retained only during the Cloudinary migration.</summary>
    public byte[]? Content { get; set; }
    public string? StorageKey { get; set; }
    public string StorageProvider { get; set; } = "Cloudinary";
    public string ResourceType { get; set; } = "raw";
    public string DeliveryType { get; set; } = "authenticated";
    public string? ChecksumSha256 { get; set; }
    public bool IsDefault { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
