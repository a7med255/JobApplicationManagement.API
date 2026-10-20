using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Jobs.Commands;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManagement.API.Controllers;

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

    /// <summary>Creates a draft or immediately published job for the current recruiter's company.</summary>
    [HttpPost]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<JobResponseDto>> CreateJob(
        [FromBody] CreateJobDto createJobDto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating job {JobTitle}", createJobDto.Title);
        var result = await _mediator.Send(new CreateJobCommand(createJobDto), cancellationToken);
        return CreatedAtAction(nameof(GetJobById), new { id = result.Id }, result);
    }

    /// <summary>Searches currently published jobs. Administrators may request inactive jobs.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResult<JobSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<JobSummaryDto>>> GetAllJobs(
        [FromQuery] JobSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetJobsQuery(request), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets all job states owned by the current recruiter.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(typeof(PaginatedResult<JobSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<JobSummaryDto>>> GetMyJobs(
        [FromQuery] JobSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyJobsQuery(request), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets complete job details and candidate-specific saved/application state.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobResponseDto>> GetJobById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetJobByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/similar")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSimilar(int id, [FromQuery] int limit = 4, CancellationToken cancellationToken = default) =>
        Ok(await _mediator.Send(new GetSimilarJobsQuery(id, limit), cancellationToken));

    [HttpGet("recommended")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetRecommended([FromQuery] int limit = 6, CancellationToken cancellationToken = default) =>
        Ok(await _mediator.Send(new GetRecommendedJobsQuery(limit), cancellationToken));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobResponseDto>> UpdateJob(
        [FromRoute] int id,
        [FromBody] UpdateJobDto updateJobDto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateJobCommand(id, updateJobDto), cancellationToken);
        return Ok(result);
    }

    /// <summary>Permanently deletes a draft that has no applications or saved-job references.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteJob([FromRoute] int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteJobCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/publish")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobResponseDto>> PublishJob([FromRoute] int id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new PublishJobCommand(id), cancellationToken));

    [HttpPut("{id:int}/close")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobResponseDto>> CloseJob([FromRoute] int id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new CloseJobCommand(id), cancellationToken));

    [HttpPut("{id:int}/reopen")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobResponseDto>> ReopenJob([FromRoute] int id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ReopenJobCommand(id), cancellationToken));

    [HttpPut("{id:int}/archive")]
    [Authorize(Roles = "Recruiter,Admin")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobResponseDto>> ArchiveJob([FromRoute] int id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ArchiveJobCommand(id), cancellationToken));
}
