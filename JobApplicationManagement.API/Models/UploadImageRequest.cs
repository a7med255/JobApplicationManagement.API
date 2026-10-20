using System.ComponentModel.DataAnnotations;

namespace JobApplicationManagement.API.Models;

public sealed class UploadImageRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
