namespace JobApplicationManagement.Application.Common.Models;

public sealed class FileUploadOptions
{
    public const string SectionName = "FileStorage";
    public long MaximumCvBytes { get; set; } = 5 * 1024 * 1024;
    public long MaximumImageBytes { get; set; } = 2 * 1024 * 1024;
}
