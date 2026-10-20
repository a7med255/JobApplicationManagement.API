using JobApplicationManagement.Application.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManagement.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminOperationsController : ControllerBase
{
    private readonly IAdminOperationsService _service;
    public AdminOperationsController(IAdminOperationsService service) => _service = service;
    [HttpGet("users")] public async Task<IActionResult> Users([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50) => Ok(await _service.GetUsersAsync(pageNumber, pageSize));
    [HttpPut("users/{userId}/lock")] public async Task<IActionResult> Lock(string userId, CancellationToken ct) { await _service.SetLockoutAsync(userId, true, ct); return NoContent(); }
    [HttpPut("users/{userId}/unlock")] public async Task<IActionResult> Unlock(string userId, CancellationToken ct) { await _service.SetLockoutAsync(userId, false, ct); return NoContent(); }
    [HttpPut("users/{userId}/roles")] public async Task<IActionResult> Roles(string userId, UpdateRolesDto dto, CancellationToken ct) { await _service.SetRolesAsync(userId, dto.Roles, ct); return NoContent(); }
    [HttpGet("outbox-failures")] public async Task<IActionResult> Failures(CancellationToken ct) => Ok(await _service.GetOutboxFailuresAsync(ct));
}
