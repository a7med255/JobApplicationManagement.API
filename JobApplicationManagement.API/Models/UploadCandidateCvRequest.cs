using System.ComponentModel.DataAnnotations;

namespace JobApplicationManagement.API.Models;

public class UploadCandidateCvRequest
{
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;

    public bool SetAsDefault { get; set; }
}
