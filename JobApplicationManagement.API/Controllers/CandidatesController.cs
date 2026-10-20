using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Features.Candidates.Commands;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Application.Features.Candidates.Queries;
using MediatR;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.API.Models;

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
    private readonly IMediator _mediator;

    public CandidatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new candidate profile.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible only by Administrators.
    /// Note: Candidates are typically created automatically upon registration.
    /// </remarks>
    /// <param name="createDto">Contains candidate details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDto createDto, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new CreateCandidateCommand(createDto), cancellationToken);
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of candidates.</returns>
    /// <response code="200">Paginated list returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the required Admin role.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResult<CandidateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllCandidates(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetAllCandidatesQuery(pageNumber, pageSize), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a candidate's profile by identifier.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the candidate who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the candidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
    public async Task<IActionResult> GetCandidateById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetCandidateByIdQuery(id), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves the authenticated candidate's profile.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible only by the Candidate.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The detailed candidate profile.</returns>
    /// <response code="200">Candidate found.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not have the Candidate role.</response>
    /// <response code="404">Candidate profile was not found.</response>
    [HttpGet("mine")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(CandidateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetMyCandidateProfileQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("mine/profile-image")]
    [Authorize(Roles = "Candidate")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CandidateResponseDto>> UploadMyProfileImage(
        [FromForm] UploadImageRequest request,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await request.File.CopyToAsync(stream, cancellationToken);
        return Ok(await _mediator.Send(new UploadMyProfileImageCommand(
            new ImageUpload(stream.ToArray(), request.File.FileName, request.File.ContentType)), cancellationToken));
    }

    /// <summary>
    /// Updates a candidate's profile.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the candidate who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the candidate.</param>
    /// <param name="updateDto">Contains updated candidate details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
    public async Task<IActionResult> UpdateCandidate(
        [FromRoute] int id,
        [FromBody] UpdateCandidateDto updateDto,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new UpdateCandidateProfileCommand(id, updateDto), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a candidate profile permanently.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Accessible by Administrators and the candidate who owns the profile.
    /// </remarks>
    /// <param name="id">The unique identifier of the candidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
    public async Task<IActionResult> DeleteCandidate([FromRoute] int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCandidateProfileCommand(id), cancellationToken);
        return NoContent();
    }
}
