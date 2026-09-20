using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Application.Features.Candidates.Interfaces;

namespace JobApplicationManagement.API.Controllers;

/// <summary>
/// Provides endpoints for managing candidates.
/// </summary>
/// <remarks>
/// Administrators have full access to manage all candidates.
/// Candidates can manage their own profiles.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;
    private readonly ICurrentUserService _currentUserService;

    public CandidatesController(ICandidateService candidateService, ICurrentUserService currentUserService)
    {
        _candidateService = candidateService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new candidate profile.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible only by Administrators.
    /// Note: Candidates are typically created automatically upon registration.
    /// </remarks>
    /// <param name="createDto">Contains candidate details.</param>
    /// <returns>The created candidate profile.</returns>
    /// <response code="201">Candidate created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the required Admin role.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CandidateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDto createDto)
    {
        var response = await _candidateService.CreateCandidateAsync(createDto);
        return CreatedAtAction(nameof(GetCandidateById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Gets a paginated list of all candidates.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible only by Administrators.
    /// </remarks>
    /// <param name="pageNumber">The page number. Default is 1.</param>
    /// <param name="pageSize">The number of candidates per page. Default is 10.</param>
    /// <returns>A paginated list of candidates.</returns>
    /// <response code="200">Paginated list returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the required Admin role.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResult<CandidateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllCandidates([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _candidateService.GetAllCandidatesAsync(pageNumber, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a candidate's profile by identifier.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the candidate who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the candidate.</param>
    /// <returns>The detailed candidate profile.</returns>
    /// <response code="200">Candidate found.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this profile.</response>
    /// <response code="404">Candidate was not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Candidate")]
    [ProducesResponseType(typeof(CandidateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidateById([FromRoute] int id)
    {
        var response = await _candidateService.GetCandidateByIdAsync(id, _currentUserService.UserId!, _currentUserService.Roles);
        return Ok(response);
    }

    /// <summary>
    /// Updates a candidate's profile.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the candidate who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the candidate.</param>
    /// <param name="updateDto">Contains updated candidate details.</param>
    /// <returns>The updated candidate profile.</returns>
    /// <response code="200">Candidate updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this profile.</response>
    /// <response code="404">Candidate was not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Candidate")]
    [ProducesResponseType(typeof(CandidateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCandidate([FromRoute] int id, [FromBody] UpdateCandidateDto updateDto)
    {
        var response = await _candidateService.UpdateCandidateAsync(id, updateDto, _currentUserService.UserId!, _currentUserService.Roles);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a candidate profile permanently.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the candidate who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the candidate.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Candidate deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this profile.</response>
    /// <response code="404">Candidate was not found.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Candidate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCandidate([FromRoute] int id)
    {
        await _candidateService.DeleteCandidateAsync(id, _currentUserService.UserId!, _currentUserService.Roles);
        return NoContent();
    }
}
