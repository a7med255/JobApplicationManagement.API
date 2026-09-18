using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;

namespace JobApplicationManagement.Application.Features.Applications.Interfaces;
/// <summary>
/// Defines application-related operations (job application, not the app layer).
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Cancels a job application. Verifies ownership and status constraints.
    /// </summary>
    /// <param name="applicationId">The job application ID.</param>
    /// <param name="userId">The authenticated user's Identity UserId.</param>
    Task<JobApplicationResponseDto> CreateApplicationAsync(CreateJobApplicationDto createDto, string userId);
    Task<PaginatedResult<JobApplicationResponseDto>> GetAllApplicationsAsync(int pageNumber, int pageSize, string userId, IEnumerable<string> roles);
    Task<JobApplicationResponseDto> GetApplicationByIdAsync(int id, string userId, IEnumerable<string> roles);
    Task<JobApplicationResponseDto> UpdateApplicationAsync(int id, UpdateJobApplicationDto updateDto, string userId, IEnumerable<string> roles);
    Task DeleteApplicationAsync(int id);
    Task CancelApplicationAsync(int applicationId, string userId);
}
