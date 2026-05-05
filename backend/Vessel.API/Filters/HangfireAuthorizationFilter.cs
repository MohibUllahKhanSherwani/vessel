using Hangfire.Dashboard;
using Vessel.Core.Enums;

namespace Vessel.API.Filters;

/// <summary>
/// Authorization filter for Hangfire dashboard to restrict access to Admins.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// Determines whether the current user can access the Hangfire dashboard.
    /// </summary>
    /// <param name="context">The Hangfire dashboard context.</param>
    /// <returns><c>true</c> when the user is authenticated as an admin.</returns>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Check if the user is authenticated and has the Admin role
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole(UserRole.Admin.ToString());
    }
}
