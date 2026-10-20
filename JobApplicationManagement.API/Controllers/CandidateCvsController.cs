using JobApplicationManagement.API.Models;
using JobApplicationManagement.Application.Features.CandidateCvs.Commands;
using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using JobApplicationManagement.Application.Features.CandidateCvs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JobApplicationManagement.Application.Common.Models;

namespace JobApplicationManagement.API.Controllers;

[ApiController]
[Route("api/candidate-cvs")]
[Authorize(Roles = "Candidate")]
public class CandidateCvsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly FileUploadOptions _options;

    public CandidateCvsController(IMediator mediator, IOptions<FileUploadOptions> options)
    {
        _mediator = mediator;
        _options = options.Value;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CandidateCvResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CandidateCvResponseDto>>> GetMine(
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetMyCandidateCvsQuery(), cancellationToken));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CandidateCvResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CandidateCvResponseDto>> Upload(
        [FromForm] UploadCandidateCvRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File.Length <= 0 || request.File.Length > _options.MaximumCvBytes)
            return BadRequest(new { message = $"CV file must be between 1 byte and {_options.MaximumCvBytes / 1024 / 1024} MB." });

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

    [HttpPut("{id:int}/name")]
    [ProducesResponseType(typeof(CandidateCvResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CandidateCvResponseDto>> Rename(
        [FromRoute] int id,
        [FromBody] RenameCandidateCvRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new RenameCandidateCvCommand(id, request.DisplayName), cancellationToken));

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
