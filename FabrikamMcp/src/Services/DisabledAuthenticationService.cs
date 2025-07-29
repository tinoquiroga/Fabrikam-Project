using System.Security.Claims;
using FabrikamMcp.Models;

namespace FabrikamMcp.Services;

/// <summary>
/// Disabled authentication service that allows all operations without authentication
/// </summary>
public class DisabledAuthenticationService : IAuthenticationService
{
    /// <summary>
    /// Returns a dummy user ID when authentication is disabled
    /// </summary>
    public string? GetCurrentUserId()
    {
        return "system";
    }

    /// <summary>
    /// Returns all roles when authentication is disabled
    /// </summary>
    public IEnumerable<string> GetCurrentUserRoles()
    {
        return new[] { "Admin", "SalesManager", "Customer" };
    }

    /// <summary>
    /// Always returns true when authentication is disabled
    /// </summary>
    public bool HasRole(string role)
    {
        return true;
    }

    /// <summary>
    /// Always returns false when authentication is disabled (system user, not authenticated user)
    /// </summary>
    public bool IsAuthenticated()
    {
        return false; // System mode, not user authentication
    }

    /// <summary>
    /// Creates a dummy authentication context when authentication is disabled
    /// </summary>
    public AuthenticationContext CreateAuthenticationContext()
    {
        return new AuthenticationContext
        {
            UserId = "system",
            UserName = "System User",
            Roles = GetCurrentUserRoles().ToList(),
            IsAuthenticated = false // Indicates this is a disabled auth context
        };
    }

    /// <summary>
    /// Returns null when authentication is disabled (no token needed)
    /// </summary>
    public string? GetCurrentJwtToken()
    {
        return null; // No token needed when authentication is disabled
    }
}
