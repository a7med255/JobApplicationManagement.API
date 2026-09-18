using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Application.Features.Recruiters.Interfaces;

namespace JobApplicationManagement.API.Controllers;

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
