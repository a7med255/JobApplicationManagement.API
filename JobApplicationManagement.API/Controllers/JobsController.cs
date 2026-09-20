using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Commands;
using JobApplicationManagement.Application.Features.Jobs.Queries;
using MediatR;

namespace JobApplicationManagement.API.Controllers;

/// <summary>
/// Provides endpoints for managing job postings.
/// </summary>
/// <remarks>
/// Allows Recruiters and Administrators to create, update, delete, close, and reopen job postings.
/// Candidates can retrieve the list of active jobs and view details.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IMediator mediator, ILogger<JobsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new job posting.
    /// </summary>
    /// <remarks>
    /// Sends a CreateJobCommand through MediatR.
    /// Requires authentication. Accessible by Recruiters and Administrators.
    /// </remarks>
    /// <param name="createJobDto">Contains the job title, description, and other job information.</param>
    /// <returns>The created job.</returns>
    /// <response code="201">Job created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not have the required role.</response>
    [HttpPost]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobDto createJobDto)
    {
        _logger.LogInformation("Creating job {JobTitle}", createJobDto.Title);

        var result = await _mediator.Send(new CreateJobCommand(createJobDto));

        return CreatedAtAction(nameof(GetJobById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Gets a paginated list of jobs.
    /// </summary>
    /// <remarks>
    /// Sends a GetJobsQuery through MediatR.
    /// Requires authentication. Accessible by all authenticated users.
    /// </remarks>
    /// <param name="pageNumber">The page number. Default is 1.</param>
    /// <param name="pageSize">The number of jobs per page. Default is 10.</param>
    /// <returns>A paginated list of jobs.</returns>
    /// <response code="200">Paginated list returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<JobResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllJobs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Fetching jobs page {Page} with size {Size}", pageNumber, pageSize);

        var result = await _mediator.Send(new GetJobsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a job by its identifier.
    /// </summary>
    /// <remarks>
    /// Sends a GetJobByIdQuery through MediatR.
    /// Requires authentication. Accessible by all authenticated users.
    /// </remarks>
    /// <param name="id">The unique identifier of the job.</param>
    /// <returns>The detailed job information.</returns>
    /// <response code="200">Job found.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Job was not found.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobById([FromRoute] int id)
    {
        _logger.LogInformation("Fetching job {JobId}", id);

        var result = await _mediator.Send(new GetJobByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing job.
    /// </summary>
    /// <remarks>
    /// Sends an UpdateJobCommand through MediatR.
    /// Requires authentication. Accessible by Administrators and the Recruiter who owns the job.
    /// </remarks>
    /// <param name="id">The unique identifier of the job.</param>
    /// <param name="updateJobDto">Contains the updated job information.</param>
    /// <returns>The updated job.</returns>
    /// <response code="200">Job updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this job.</response>
    /// <response code="404">Job was not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateJob([FromRoute] int id, [FromBody] UpdateJobDto updateJobDto)
    {
        _logger.LogInformation("Updating job {JobId}", id);

        var result = await _mediator.Send(new UpdateJobCommand(id, updateJobDto));
        return Ok(result);
    }

    /// <summary>
    /// Deletes a job permanently.
    /// </summary>
    /// <remarks>
    /// Sends a DeleteJobCommand through MediatR.
    /// Requires authentication. Accessible by Administrators and the Recruiter who owns the job.
    /// </remarks>
    /// <param name="id">The unique identifier of the job.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Job deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this job.</response>
    /// <response code="404">Job was not found.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJob([FromRoute] int id)
    {
        _logger.LogInformation("Deleting job {JobId}", id);

        await _mediator.Send(new DeleteJobCommand(id));
        return NoContent();
    }

    /// <summary>
    /// Closes a job posting.
    /// </summary>
    /// <remarks>
    /// Sends a CloseJobCommand through MediatR.
    /// Requires authentication. Accessible by Administrators and the Recruiter who owns the job.
    /// Closed jobs do not accept new applications.
    /// </remarks>
    /// <param name="id">The unique identifier of the job.</param>
    /// <returns>Information about the closed job.</returns>
    /// <response code="200">Job closed successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this job.</response>
    /// <response code="404">Job was not found.</response>
    [HttpPut("{id:int}/close")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(CloseJobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseJob([FromRoute] int id)
    {
        _logger.LogInformation("Closing job {JobId}", id);

        var result = await _mediator.Send(new CloseJobCommand(id));
        return Ok(result);
    }

    /// <summary>
    /// Reopens a closed job posting.
    /// </summary>
    /// <remarks>
    /// Sends a ReopenJobCommand through MediatR.
    /// Requires authentication. Accessible by Administrators and the Recruiter who owns the job.
    /// Reopened jobs can accept new applications again.
    /// </remarks>
    /// <param name="id">The unique identifier of the job.</param>
    /// <returns>The reopened job.</returns>
    /// <response code="200">Job reopened successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own this job.</response>
    /// <response code="404">Job was not found.</response>
    [HttpPut("{id:int}/reopen")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReopenJob([FromRoute] int id)
    {
        _logger.LogInformation("Reopening job {JobId}", id);

        var result = await _mediator.Send(new ReopenJobCommand(id));
        return Ok(result);
    }
}
