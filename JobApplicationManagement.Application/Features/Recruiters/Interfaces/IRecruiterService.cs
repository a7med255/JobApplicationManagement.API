using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;

namespace JobApplicationManagement.Application.Features.Recruiters.Interfaces;

public interface IRecruiterService
{
    Task<RecruiterResponseDto> CreateRecruiterAsync(CreateRecruiterDto createDto);
    Task<PaginatedResult<RecruiterResponseDto>> GetAllRecruitersAsync(int pageNumber, int pageSize);
    Task<RecruiterResponseDto> GetRecruiterByIdAsync(int id, string userId, IEnumerable<string> roles);
    Task<RecruiterResponseDto> UpdateRecruiterAsync(int id, UpdateRecruiterDto updateDto, string userId, IEnumerable<string> roles);
    Task DeleteRecruiterAsync(int id, string userId, IEnumerable<string> roles);
}
