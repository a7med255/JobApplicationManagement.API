using JobApplicationManagement.Application.Features.Interviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManagement.API.Controllers;

[ApiController]
[Route("api/interviews")]
[Authorize]
public sealed class InterviewsController : ControllerBase
{
    private readonly IInterviewService _service;
    public InterviewsController(IInterviewService service) => _service = service;

    [HttpGet("application/{applicationId:int}")]
    public async Task<IActionResult> GetForApplication(int applicationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetForApplicationAsync(applicationId, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Schedule(ScheduleInterviewDto request, CancellationToken cancellationToken)
    {
        var result = await _service.ScheduleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetForApplication), new { applicationId = result.ApplicationId }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Reschedule(int id, RescheduleInterviewDto request, CancellationToken cancellationToken) =>
        Ok(await _service.RescheduleAsync(id, request, cancellationToken));

    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _service.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
