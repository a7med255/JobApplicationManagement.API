using JobApplicationManagement.Application.Features.Companies.Commands;
using JobApplicationManagement.Application.Features.Companies.DTOs;
using JobApplicationManagement.Application.Features.Companies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.API.Models;

namespace JobApplicationManagement.API.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize(Roles = "Recruiter")]
public class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("mine")]
    [ProducesResponseType(typeof(CompanyResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyResponseDto>> GetMine(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetMyCompanyQuery(), cancellationToken));

    [HttpPut("mine")]
    [ProducesResponseType(typeof(CompanyResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyResponseDto>> UpsertMine(
        [FromBody] UpdateCompanyDto company,
        CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new UpsertMyCompanyCommand(company), cancellationToken));

    [HttpPost("mine/logo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CompanyResponseDto>> UploadLogo(
        [FromForm] UploadImageRequest request,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await request.File.CopyToAsync(stream, cancellationToken);
        return Ok(await _mediator.Send(new UploadMyCompanyLogoCommand(
            new ImageUpload(stream.ToArray(), request.File.FileName, request.File.ContentType)), cancellationToken));
    }
}
