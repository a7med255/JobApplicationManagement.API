using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Application.Features.Applications.Interfaces;

namespace JobApplicationManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(
        IApplicationService applicationService,
        ICurrentUserService currentUserService,
        ILogger<ApplicationsController> logger)
    {
        _applicationService = applicationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

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
        var response = await _applicationService.CreateApplicationAsync(createDto, _currentUserService.UserId!);
        return CreatedAtAction(nameof(GetApplicationById), new { id = response.Id }, response);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter,Candidate")]
    [ProducesResponseType(typeof(PaginatedResult<JobApplicationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllApplications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _applicationService.GetAllApplicationsAsync(pageNumber, pageSize, _currentUserService.UserId!, _currentUserService.Roles);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Recruiter,Candidate")]
    [ProducesResponseType(typeof(JobApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplicationById([FromRoute] int id)
    {
        var response = await _applicationService.GetApplicationByIdAsync(id, _currentUserService.UserId!, _currentUserService.Roles);
        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(JobApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApplication([FromRoute] int id, [FromBody] UpdateJobApplicationDto updateDto)
    {
        var response = await _applicationService.UpdateApplicationAsync(id, updateDto, _currentUserService.UserId!, _currentUserService.Roles);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteApplication([FromRoute] int id)
    {
        await _applicationService.DeleteApplicationAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelApplication([FromRoute] int id)
    {
        _logger.LogInformation(
            "PUT /api/applications/{ApplicationId}/cancel called by user {UserId}",
            id, _currentUserService.UserId);

        await _applicationService.CancelApplicationAsync(id, _currentUserService.UserId!);

        return NoContent();
    }
}
