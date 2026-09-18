namespace JobApplicationManagement.Application.Features.Jobs.DTOs;

/// <summary>
/// Data Transfer Object returned to the client after a successful Job operation.
/// Returns the domain entity's data without exposing the entity itself,
/// which keeps API contracts stable even when the domain model evolves.
/// </summary>
public class JobResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
