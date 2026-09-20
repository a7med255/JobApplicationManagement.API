using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Application.Features.Applications.Commands;
using JobApplicationManagement.Application.Features.Applications.Queries;
using MediatR;

namespace JobApplicationManagement.API.Controllers;

/// <summary>
/// Provides endpoints for managing job applications.
/// </summary>
/// <remarks>
/// Candidates can manage their own applications, while Recruiters can manage applications related to their jobs.
/// Administrators have full access.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<ApplicationsController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Applies for a job.
    /// </summary>
    /// <remarks>
    /// Sends an ApplyForJobCommand through MediatR.
    /// Requires authentication. Accessible only by Candidates.
    /// </remarks>
    /// <param name="createDto">Contains the job ID to apply for.</param>
    /// <returns>The created job application.</returns>
    /// <response code="201">Application submitted successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the required role.</response>
    /// <response code="404">Job was not found.</response>
    /// <response code="409">Candidate has already applied, or the job is closed.</response>
    [HttpPost]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(JobApplicationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateApplication([FromBody] CreateJobApplicationDto createDto)
    {
        _logger.LogInformation("Creating application for Job {JobId}", createDto.JobId);

        var response = await _mediator.Send(new ApplyForJobCommand(createDto));
        return CreatedAtAction(nameof(GetApplicationById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Gets a paginated list of job applications.
    /// </summary>
    /// <remarks>
    /// Dynamically routes to GetAllApplicationsQuery, GetJobApplicationsQuery, or GetMyApplicationsQuery based on the user's role.
    /// Requires authentication. Accessible by all authenticated users, but results are scoped to their permissions.
    /// </remarks>
    /// <param name="pageNumber">The page number. Default is 1.</param>
    /// <param name="pageSize">The number of applications per page. Default is 10.</param>
    /// <returns>A paginated list of job applications.</returns>
    /// <response code="200">Paginated list returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter,Candidate")]
    [ProducesResponseType(typeof(PaginatedResult<JobApplicationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllApplications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var roles = _currentUserService.Roles.ToList();
        PaginatedResult<JobApplicationResponseDto> response;

        if (roles.Contains("Admin"))
        {
            response = await _mediator.Send(new GetAllApplicationsQuery(pageNumber, pageSize));
        }
        else if (roles.Contains("Recruiter"))
        {
            response = await _mediator.Send(new GetJobApplicationsQuery(pageNumber, pageSize));
        }
        else // Candidate
        {
            response = await _mediator.Send(new GetMyApplicationsQuery(pageNumber, pageSize));
        }

        return Ok(response);
    }

    /// <summary>
    /// Retrieves a job application by its identifier.
    /// </summary>
    /// <remarks>
    /// Sends a GetApplicationByIdQuery through MediatR.
    /// Requires authentication. Accessible by Administrators, the Candidate who applied, or the Recruiter who owns the job.
    /// </remarks>
    /// <param name="id">The unique identifier of the application.</param>
    /// <returns>The detailed job application.</returns>
    /// <response code="200">Application found.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not have permission to view this application.</response>
    /// <response code="404">Application was not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Recruiter,Candidate")]
    [ProducesResponseType(typeof(JobApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplicationById([FromRoute] int id)
    {
        var response = await _mediator.Send(new GetApplicationByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Updates the status of a job application.
    /// </summary>
    /// <remarks>
    /// Sends an UpdateApplicationStatusCommand through MediatR.
    /// Requires authentication. Accessible by Administrators and the Recruiter who owns the associated job.
    /// </remarks>
    /// <param name="id">The unique identifier of the application.</param>
    /// <param name="updateDto">Contains the new status.</param>
    /// <returns>The updated job application.</returns>
    /// <response code="200">Application updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own the associated job.</response>
    /// <response code="404">Application was not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(JobApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApplication([FromRoute] int id, [FromBody] UpdateJobApplicationDto updateDto)
    {
        var response = await _mediator.Send(new UpdateApplicationStatusCommand(id, updateDto));
        return Ok(response);
    }

    /// <summary>
    /// Deletes a job application permanently.
    /// </summary>
    /// <remarks>
    /// Sends a DeleteApplicationCommand through MediatR.
    /// Requires authentication. Accessible only by Administrators.
    /// </remarks>
    /// <param name="id">The unique identifier of the application.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Application deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User lacks the required role.</response>
    /// <response code="404">Application was not found.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteApplication([FromRoute] int id)
    {
        await _mediator.Send(new DeleteApplicationCommand(id));
        return NoContent();
    }

    /// <summary>
    /// Cancels a job application.
    /// </summary>
    /// <remarks>
    /// Sends a CancelApplicationCommand through MediatR.
    /// The authenticated candidate can cancel only their own application.
    /// Cancellation is allowed only when the application status is Applied or UnderReview.
    /// </remarks>
    /// <param name="id">The unique identifier of the application.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Application cancelled successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">The authenticated user does not own the application.</response>
    /// <response code="404">Application was not found.</response>
    /// <response code="409">Application cannot be cancelled in its current status.</response>
    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelApplication([FromRoute] int id)
    {
        _logger.LogInformation("Cancelling application {ApplicationId}", id);

        await _mediator.Send(new CancelApplicationCommand(id));
        return NoContent();
    }
}
