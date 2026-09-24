using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.SavedJobs.Commands;
using JobApplicationManagement.Application.Features.SavedJobs.DTOs;
using JobApplicationManagement.Application.Features.SavedJobs.Queries;
using MediatR;

namespace JobApplicationManagement.API.Controllers;

/// <summary>
/// Provides endpoints for managing a candidate's saved/bookmarked jobs.
/// </summary>
/// <remarks>
/// Allows authenticated Candidates to save, remove, list, and check saved status of jobs.
/// CandidateId is always resolved from the authenticated user's claims — never from the request.
/// </remarks>
[ApiController]
[Route("api/jobs")]
public class SavedJobsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SavedJobsController> _logger;

    public SavedJobsController(IMediator mediator, ILogger<SavedJobsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Saves a job for the authenticated candidate.
    /// </summary>
    /// <remarks>
    /// Sends a SaveJobCommand through MediatR.
    /// Requires authentication. Accessible only by Candidates.
    /// Does NOT create a job application.
    /// </remarks>
    /// <param name="jobId">The unique identifier of the job to save.</param>
    /// <returns>The saved job details.</returns>
    /// <response code="201">Job saved successfully.</response>
    /// <response code="400">Validation failed (e.g., invalid JobId).</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the Candidate role or is not a registered candidate.</response>
    /// <response code="404">Job was not found.</response>
    /// <response code="409">Job is already saved by this candidate.</response>
    [HttpPost("{jobId:int}/save")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(SavedJobResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveJob([FromRoute] int jobId)
    {
        _logger.LogInformation("Saving job {JobId}", jobId);

        var result = await _mediator.Send(new SaveJobCommand(jobId));
        return CreatedAtAction(nameof(GetSavedJobStatus), new { jobId }, result);
    }

    /// <summary>
    /// Removes a saved job for the authenticated candidate.
    /// </summary>
    /// <remarks>
    /// Sends a RemoveSavedJobCommand through MediatR.
    /// Requires authentication. Accessible only by Candidates.
    /// Does NOT affect any existing job application.
    /// </remarks>
    /// <param name="jobId">The unique identifier of the job to unsave.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Saved job removed successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the Candidate role or is not a registered candidate.</response>
    /// <response code="404">Saved job was not found.</response>
    [HttpDelete("{jobId:int}/save")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSavedJob([FromRoute] int jobId)
    {
        _logger.LogInformation("Removing saved job {JobId}", jobId);

        await _mediator.Send(new RemoveSavedJobCommand(jobId));
        return NoContent();
    }

    /// <summary>
    /// Gets the authenticated candidate's saved jobs (paginated).
    /// </summary>
    /// <remarks>
    /// Sends a GetMySavedJobsQuery through MediatR.
    /// Requires authentication. Accessible only by Candidates.
    /// Returns job details for each saved job.
    /// </remarks>
    /// <param name="pageNumber">The page number. Default is 1.</param>
    /// <param name="pageSize">The number of saved jobs per page. Default is 10.</param>
    /// <returns>A paginated list of saved jobs.</returns>
    /// <response code="200">Paginated list returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the Candidate role.</response>
    [HttpGet("saved")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(PaginatedResult<SavedJobResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMySavedJobs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Fetching saved jobs page {Page} with size {Size}", pageNumber, pageSize);

        var result = await _mediator.Send(new GetMySavedJobsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Checks whether a specific job is saved by the authenticated candidate.
    /// </summary>
    /// <remarks>
    /// Sends an IsJobSavedQuery through MediatR.
    /// Requires authentication. Accessible only by Candidates.
    /// </remarks>
    /// <param name="jobId">The unique identifier of the job to check.</param>
    /// <returns>An object with a boolean 'isSaved' property.</returns>
    /// <response code="200">Status returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the Candidate role.</response>
    [HttpGet("{jobId:int}/saved-status")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSavedJobStatus([FromRoute] int jobId)
    {
        var isSaved = await _mediator.Send(new IsJobSavedQuery(jobId));
        return Ok(new { isSaved });
    }
}
