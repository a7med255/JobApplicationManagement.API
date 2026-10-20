using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs;

public static class JobAuthorizationHelper
{
    private const string AdminRole = "Admin";

    public static async Task VerifyOwnershipOrAdminAsync(
        IUnitOfWork unitOfWork,
        Job job,
        ICurrentUserService currentUserService,
        string operation,
        CancellationToken cancellationToken = default)
    {
        if (!currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(currentUserService.UserId))
            throw new UnauthorizedException("Authentication is required to perform this action.");

        var roles = currentUserService.Roles.ToList();
        if (roles.Contains(AdminRole))
            return;

        var userId = currentUserService.UserId;
        var recruiter = await unitOfWork.Recruiters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

        if (recruiter is null || job.RecruiterId != recruiter.Id)
        {
            throw new ForbiddenException($"You do not have permission to {operation} this job.");
        }
    }
}
