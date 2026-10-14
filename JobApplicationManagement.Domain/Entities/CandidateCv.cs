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
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public bool IsDefault { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime UploadedAt { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
