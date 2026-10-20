using JobApplicationManagement.Application.Features.JobAlerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManagement.API.Controllers;

[ApiController]
[Route("api/job-alerts")]
[Authorize(Roles = "Candidate")]
public sealed class JobAlertsController : ControllerBase
{
    private readonly IJobAlertService _service;
    public JobAlertsController(IJobAlertService service) => _service = service;
    [HttpGet] public async Task<IActionResult> GetMine(CancellationToken ct) => Ok(await _service.GetMineAsync(ct));
    [HttpPost] public async Task<IActionResult> Create(SaveJobAlertDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetMine), new { id = result.Id }, result);
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, SaveJobAlertDto dto, CancellationToken ct) => Ok(await _service.UpdateAsync(id, dto, ct));
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) { await _service.DeleteAsync(id, ct); return NoContent(); }
}
