using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Jobs.DTOs;

namespace JobApplicationManagement.Application.Features.Jobs.Interfaces;

/// <summary>
/// Defines the application operations available for Jobs.
/// </summary>
public interface IJobService
{
    Task<JobResponseDto> CreateJobAsync(CreateJobDto createJobDto, string createdByUserId);
    Task<PaginatedResult<JobResponseDto>> GetAllJobsAsync(int pageNumber, int pageSize);
    Task<JobResponseDto> GetJobByIdAsync(int id);
    Task<JobResponseDto> UpdateJobAsync(int id, UpdateJobDto updateJobDto, string userId, IEnumerable<string> roles);
    Task DeleteJobAsync(int id, string userId, IEnumerable<string> roles);
    Task<CloseJobResponseDto> CloseJobAsync(int id, string userId, IEnumerable<string> roles);
}
