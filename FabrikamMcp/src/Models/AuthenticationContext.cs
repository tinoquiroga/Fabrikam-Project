namespace FabrikamMcp.Models;

/// <summary>
/// Authentication context for MCP tool operations
/// Used for logging, auditing, and authorization decisions
/// </summary>
public class AuthenticationContext
{
    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// User ID from JWT token
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// User name from JWT token
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// User roles from JWT token
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Check if user has a specific role
    /// </summary>
    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if user has any of the specified roles
    /// </summary>
    public bool HasAnyRole(params string[] roles)
    {
        return roles.Any(role => HasRole(role));
    }

    /// <summary>
    /// Get a user-friendly display name for logging
    /// </summary>
    public string GetDisplayName()
    {
        if (!IsAuthenticated) return "Anonymous";
        return UserName ?? UserId ?? "Unknown User";
    }
}
