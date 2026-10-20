using JobApplicationManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Notifications;

public static class NotificationPreferenceRules
{
    public static async Task<bool> AllowsInAppAsync(IUnitOfWork unitOfWork, string userId, CancellationToken cancellationToken)
    {
        var preferences = await unitOfWork.NotificationPreferences.Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        return preferences?.InAppEnabled ?? true;
    }
}
