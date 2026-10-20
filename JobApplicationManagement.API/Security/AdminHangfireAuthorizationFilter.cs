using Hangfire.Dashboard;

namespace JobApplicationManagement.API.Security;

/// <summary>Restricts operational job data to authenticated administrators.</summary>
public sealed class AdminHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
    }
}
