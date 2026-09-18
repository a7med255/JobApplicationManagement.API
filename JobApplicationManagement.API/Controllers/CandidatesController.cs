using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Application.Features.Candidates.Interfaces;

namespace JobApplicationManagement.API.Controllers;

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
