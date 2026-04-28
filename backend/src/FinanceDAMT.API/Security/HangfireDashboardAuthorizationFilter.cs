using Hangfire.Dashboard;

namespace FinanceDAMT.API.Security;

/// <summary>
/// Authorizes access to Hangfire dashboard by environment-specific rules.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="HangfireDashboardAuthorizationFilter"/> class.
    /// </summary>
    /// <param name="environment">Current host environment.</param>
    public HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Determines whether the current request can access Hangfire dashboard.
    /// </summary>
    /// <param name="context">Dashboard context wrapping HTTP request data.</param>
    /// <returns>True if access is allowed; otherwise false.</returns>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (_environment.IsDevelopment())
        {
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            return remoteIp is not null && System.Net.IPAddress.IsLoopback(remoteIp);
        }

        var user = httpContext.User;
        return user.Identity?.IsAuthenticated == true && user.IsInRole("Admin");
    }
}
