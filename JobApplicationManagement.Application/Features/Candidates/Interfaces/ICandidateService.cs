using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Candidates.DTOs;

namespace JobApplicationManagement.Application.Features.Candidates.Interfaces;

public interface ICandidateService
{
    Task<CandidateResponseDto> CreateCandidateAsync(CreateCandidateDto createDto);
    Task<PaginatedResult<CandidateResponseDto>> GetAllCandidatesAsync(int pageNumber, int pageSize);
    Task<CandidateResponseDto> GetCandidateByIdAsync(int id, string userId, IEnumerable<string> roles);
    Task<CandidateResponseDto> UpdateCandidateAsync(int id, UpdateCandidateDto updateDto, string userId, IEnumerable<string> roles);
    Task DeleteCandidateAsync(int id, string userId, IEnumerable<string> roles);
}
