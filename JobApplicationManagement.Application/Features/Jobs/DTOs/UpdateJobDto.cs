namespace JobApplicationManagement.Application.Features.Jobs.DTOs;

/// <summary>
/// DTO for updating an existing Job.
/// Same fields as CreateJobDto — Id and IsActive remain server-controlled.
/// </summary>
public class UpdateJobDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
