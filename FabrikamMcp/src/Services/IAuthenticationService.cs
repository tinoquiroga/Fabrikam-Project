using System.Security.Claims;
using FabrikamMcp.Models;

namespace FabrikamMcp.Services;

/// <summary>
/// Service interface for handling authentication context and user information
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Gets the current user's ID from the authentication context
    /// </summary>
    /// <returns>User ID if authenticated, null otherwise</returns>
    string? GetCurrentUserId();

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    /// <returns>List of user roles</returns>
    IEnumerable<string> GetCurrentUserRoles();

    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role</returns>
    bool HasRole(string role);

    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    /// <returns>True if authenticated</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Creates an authentication context from the current HTTP context
    /// </summary>
    /// <returns>Authentication context</returns>
    AuthenticationContext CreateAuthenticationContext();
}
