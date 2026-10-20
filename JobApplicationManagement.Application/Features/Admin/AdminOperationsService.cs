using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Admin;

public sealed record UpdateRolesDto(IReadOnlyCollection<string> Roles);
public sealed record OutboxFailureDto(Guid Id, string Type, int AttemptCount, string? LastError, DateTime OccurredAt, DateTime? NextAttemptAt);

public interface IAdminOperationsService
{
    Task<IReadOnlyCollection<IdentityUserInfo>> GetUsersAsync(int pageNumber, int pageSize);
    Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken);
    Task SetRolesAsync(string userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OutboxFailureDto>> GetOutboxFailuresAsync(CancellationToken cancellationToken);
}

public sealed class AdminOperationsService : IAdminOperationsService
{
    private static readonly string[] AllowedRoles = ["Candidate", "Recruiter", "Admin"];
    private readonly IIdentityService _identity;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public AdminOperationsService(IIdentityService identity, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_identity, _unitOfWork, _currentUser) = (identity, unitOfWork, currentUser);

    public Task<IReadOnlyCollection<IdentityUserInfo>> GetUsersAsync(int pageNumber, int pageSize) =>
        _identity.GetUsersAsync((Math.Max(1, pageNumber) - 1) * Math.Clamp(pageSize, 1, 100), Math.Clamp(pageSize, 1, 100));

    public async Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken)
    {
        if (userId == _currentUser.UserId && locked) throw new ConflictException("You cannot lock your own account.");
        if (await _identity.GetUserByIdAsync(userId) is null) throw new NotFoundException("IdentityUser", userId);
        await _identity.SetUserLockoutAsync(userId, locked);
        await AuditAsync(locked ? "LockUser" : "UnlockUser", userId, null, cancellationToken);
    }

    public async Task SetRolesAsync(string userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var normalized = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (normalized.Length == 0 || normalized.Any(x => !AllowedRoles.Contains(x, StringComparer.OrdinalIgnoreCase)))
            throw new Common.Exceptions.ValidationException(new Dictionary<string, string[]> { ["Roles"] = ["At least one valid role is required."] });
        if (userId == _currentUser.UserId && !normalized.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            throw new ConflictException("You cannot remove your own Admin role.");
        if (await _identity.GetUserByIdAsync(userId) is null) throw new NotFoundException("IdentityUser", userId);
        await _identity.UpdateUserRolesAsync(userId, normalized);
        await AuditAsync("UpdateRoles", userId, string.Join(',', normalized), cancellationToken);
    }

    public async Task<IReadOnlyCollection<OutboxFailureDto>> GetOutboxFailuresAsync(CancellationToken cancellationToken) =>
        await _unitOfWork.OutboxMessages.Query().AsNoTracking()
            .Where(x => x.ProcessedAt == null && x.AttemptCount > 0)
            .OrderByDescending(x => x.OccurredAt).Take(100)
            .Select(x => new OutboxFailureDto(x.Id, x.Type, x.AttemptCount, x.LastError, x.OccurredAt, x.NextAttemptAt))
            .ToListAsync(cancellationToken);

    private async Task AuditAsync(string action, string targetId, string? details, CancellationToken cancellationToken)
    {
        await _unitOfWork.AdminAuditLogs.AddAsync(new AdminAuditLog
        {
            AdminUserId = _currentUser.UserId ?? throw new ForbiddenException("An authenticated administrator is required."),
            Action = action, TargetType = "IdentityUser", TargetId = targetId, Details = details, CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
