using JobApplicationManagement.API.Models;
using JobApplicationManagement.Application.Features.CandidateCvs.Commands;
using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using JobApplicationManagement.Application.Features.CandidateCvs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManagement.API.Controllers;

[ApiController]
[Route("api/candidate-cvs")]
[Authorize(Roles = "Candidate")]
public class CandidateCvsController : ControllerBase
{
    private const long MaximumFileSize = 5 * 1024 * 1024;
    private readonly IMediator _mediator;

    public CandidateCvsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CandidateCvResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CandidateCvResponseDto>>> GetMine(
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetMyCandidateCvsQuery(), cancellationToken));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumFileSize + 64 * 1024)]
    [ProducesResponseType(typeof(CandidateCvResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CandidateCvResponseDto>> Upload(
        [FromForm] UploadCandidateCvRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File.Length is <= 0 or > MaximumFileSize)
            return BadRequest(new { message = "CV file must be between 1 byte and 5 MB." });

        await using var stream = request.File.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var result = await _mediator.Send(new UploadCandidateCvCommand(
            request.DisplayName,
            request.File.FileName,
            request.File.ContentType,
            memory.ToArray(),
            request.SetAsDefault), cancellationToken);

        return CreatedAtAction(nameof(Download), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}/default")]
    [ProducesResponseType(typeof(CandidateCvResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CandidateCvResponseDto>> SetDefault(
        [FromRoute] int id,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new SetDefaultCandidateCvCommand(id), cancellationToken));

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download([FromRoute] int id, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(new DownloadMyCandidateCvQuery(id), cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCandidateCvCommand(id), cancellationToken);
        return NoContent();
    }
}
