using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Application.Features.Recruiters.Interfaces;

namespace JobApplicationManagement.API.Controllers;

/// <summary>
/// Provides endpoints for managing recruiters.
/// </summary>
/// <remarks>
/// Administrators have full access to manage all recruiters.
/// Recruiters can manage their own profiles.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class RecruitersController : ControllerBase
{
    private readonly IRecruiterService _recruiterService;
    private readonly ICurrentUserService _currentUserService;

    public RecruitersController(IRecruiterService recruiterService, ICurrentUserService currentUserService)
    {
        _recruiterService = recruiterService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new recruiter profile.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible only by Administrators.
    /// Note: Recruiters are typically created automatically upon registration.
    /// </remarks>
    /// <param name="createDto">Contains recruiter details.</param>
    /// <returns>The created recruiter profile.</returns>
    /// <response code="201">Recruiter created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the required Admin role.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RecruiterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRecruiter([FromBody] CreateRecruiterDto createDto)
    {
        var response = await _recruiterService.CreateRecruiterAsync(createDto);
        return CreatedAtAction(nameof(GetRecruiterById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Gets a paginated list of all recruiters.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible only by Administrators.
    /// </remarks>
    /// <param name="pageNumber">The page number. Default is 1.</param>
    /// <param name="pageSize">The number of recruiters per page. Default is 10.</param>
    /// <returns>A paginated list of recruiters.</returns>
    /// <response code="200">Paginated list returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the required Admin role.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResult<RecruiterResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllRecruiters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _recruiterService.GetAllRecruitersAsync(pageNumber, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a recruiter's profile by identifier.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the recruiter who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the recruiter.</param>
    /// <returns>The detailed recruiter profile.</returns>
    /// <response code="200">Recruiter found.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this profile.</response>
    /// <response code="404">Recruiter was not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(RecruiterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecruiterById([FromRoute] int id)
    {
        var response = await _recruiterService.GetRecruiterByIdAsync(id, _currentUserService.UserId!, _currentUserService.Roles);
        return Ok(response);
    }

    /// <summary>
    /// Updates a recruiter's profile.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the recruiter who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the recruiter.</param>
    /// <param name="updateDto">Contains updated recruiter details.</param>
    /// <returns>The updated recruiter profile.</returns>
    /// <response code="200">Recruiter updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this profile.</response>
    /// <response code="404">Recruiter was not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(RecruiterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRecruiter([FromRoute] int id, [FromBody] UpdateRecruiterDto updateDto)
    {
        var response = await _recruiterService.UpdateRecruiterAsync(id, updateDto, _currentUserService.UserId!, _currentUserService.Roles);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a recruiter profile permanently.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the recruiter who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the recruiter.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Recruiter deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this profile.</response>
    /// <response code="404">Recruiter was not found.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecruiter([FromRoute] int id)
    {
        await _recruiterService.DeleteRecruiterAsync(id, _currentUserService.UserId!, _currentUserService.Roles);
        return NoContent();
    }
}
