using System.Security.Claims;
using FabrikamMcp.Models;

namespace FabrikamMcp.Services;

/// <summary>
/// Service for handling authentication context and user information in the MCP server
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's ID from the authentication context
    /// </summary>
    /// <returns>User ID if authenticated, null otherwise</returns>
    public string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               user.FindFirst("sub")?.Value; // JWT standard claim
    }

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    /// <returns>List of user roles</returns>
    public IEnumerable<string> GetCurrentUserRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return Enumerable.Empty<string>();

        return user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role</returns>
    public bool HasRole(string role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        return user.IsInRole(role);
    }

    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    /// <returns>True if authenticated</returns>
    public bool IsAuthenticated()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Creates an authentication context from the current HTTP context
    /// </summary>
    /// <returns>Authentication context</returns>
    public AuthenticationContext CreateAuthenticationContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAuthenticated = user?.Identity?.IsAuthenticated == true;

        var context = new AuthenticationContext
        {
            IsAuthenticated = isAuthenticated,
            UserId = GetCurrentUserId(),
            UserName = user?.Identity?.Name,
            Roles = GetCurrentUserRoles().ToList()
        };

        if (isAuthenticated)
        {
            _logger.LogDebug("Created authentication context for user {UserId} ({UserName}) with roles {Roles}", 
                context.UserId, context.UserName, string.Join(", ", context.Roles));
        }

        return context;
    }

    /// <summary>
    /// Gets the current JWT token for forwarding to API calls
    /// </summary>
    /// <returns>JWT token if available, null otherwise</returns>
    public string? GetCurrentJwtToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // Try to get token from Authorization header
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader["Bearer ".Length..].Trim();
        }

        _logger.LogDebug("No JWT token found in current request context");
        return null;
    }
}
