using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Interfaces;

namespace JobApplicationManagement.API.Controllers;

/// <summary>
/// API controller for Job operations.
/// Thin controller — delegates entirely to IJobService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobService jobService, ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>Creates a new job posting.</summary>
    /// <response code="201">Job created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">User does not have the required role.</response>
    [HttpPost]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobDto createJobDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("POST /api/jobs called by user {UserId} with title {JobTitle}", userId, createJobDto.Title);

        var result = await _jobService.CreateJobAsync(createJobDto, userId);

        return CreatedAtAction(nameof(GetJobById), new { id = result.Id }, result);
    }

    /// <summary>Returns a paginated list of all jobs.</summary>
    /// <response code="200">Paginated list returned.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<JobResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllJobs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("GET /api/jobs called — page {Page}, size {Size}", pageNumber, pageSize);

        var result = await _jobService.GetAllJobsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>Returns a single job by ID.</summary>
    /// <response code="200">Job found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Job not found.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobById([FromRoute] int id)
    {
        _logger.LogInformation("GET /api/jobs/{JobId} called", id);

        var result = await _jobService.GetJobByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Updates an existing job.</summary>
    /// <response code="200">Job updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">User does not own this job.</response>
    /// <response code="404">Job not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateJob([FromRoute] int id, [FromBody] UpdateJobDto updateJobDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        _logger.LogInformation("PUT /api/jobs/{JobId} called by user {UserId}", id, userId);

        var result = await _jobService.UpdateJobAsync(id, updateJobDto, userId, roles);
        return Ok(result);
    }

    /// <summary>Deletes a job by ID.</summary>
    /// <response code="204">Job deleted.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">User does not own this job.</response>
    /// <response code="404">Job not found.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJob([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        _logger.LogInformation("DELETE /api/jobs/{JobId} called by user {UserId}", id, userId);

        await _jobService.DeleteJobAsync(id, userId, roles);
        return NoContent();
    }

    /// <summary>
    /// Closes a job posting. Only the owning Recruiter or Admin can close it.
    /// </summary>
    /// <response code="200">Job closed successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Authenticated user does not own this job.</response>
    /// <response code="404">Job not found.</response>
    [HttpPut("{id:int}/close")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(CloseJobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseJob([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        _logger.LogInformation("PUT /api/jobs/{JobId}/close called by user {UserId}", id, userId);

        var result = await _jobService.CloseJobAsync(id, userId, roles);
        return Ok(result);
    }
}
