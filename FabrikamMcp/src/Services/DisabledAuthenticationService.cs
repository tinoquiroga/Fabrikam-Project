using System.Security.Claims;
using FabrikamMcp.Models;

namespace FabrikamMcp.Services;

/// <summary>
/// Disabled authentication service that provides service JWT with GUID context
/// </summary>
public class DisabledAuthenticationService : IAuthenticationService
{
    private readonly IServiceJwtService _serviceJwtService;
    private readonly ILogger<DisabledAuthenticationService> _logger;
    private string? _currentUserGuid;

    public DisabledAuthenticationService(
        IServiceJwtService serviceJwtService,
        ILogger<DisabledAuthenticationService> logger)
    {
        _serviceJwtService = serviceJwtService;
        _logger = logger;
    }
    /// <summary>
    /// Set the current user GUID context for service JWT generation
    /// </summary>
    public void SetUserGuidContext(string userGuid)
    {
        if (string.IsNullOrWhiteSpace(userGuid))
        {
            _logger.LogWarning("Attempted to set empty user GUID context");
            return;
        }

        if (!Guid.TryParse(userGuid, out var guidValue) || guidValue == Guid.Empty)
        {
            _logger.LogWarning("Invalid GUID format provided: {UserGuid}", userGuid);
            return;
        }

        _currentUserGuid = userGuid;
        _logger.LogDebug("Set user GUID context: {UserGuid}", userGuid);
    }

    /// <summary>
    /// Returns a user ID based on current GUID context
    /// </summary>
    public string? GetCurrentUserId()
    {
        return !string.IsNullOrEmpty(_currentUserGuid) ? $"disabled-user-{_currentUserGuid}" : "system";
    }

    /// <summary>
    /// Returns all roles when authentication is disabled
    /// </summary>
    public IEnumerable<string> GetCurrentUserRoles()
    {
        return new[] { "DisabledModeUser", "ServiceUser" };
    }

    /// <summary>
    /// Always returns true when authentication is disabled
    /// </summary>
    public bool HasRole(string role)
    {
        return true;
    }

    /// <summary>
    /// Returns true when user GUID context is set
    /// </summary>
    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(_currentUserGuid);
    }

    /// <summary>
    /// Creates authentication context with current GUID
    /// </summary>
    public AuthenticationContext CreateAuthenticationContext()
    {
        return new AuthenticationContext
        {
            UserId = GetCurrentUserId() ?? "system",
            UserName = !string.IsNullOrEmpty(_currentUserGuid) ? $"Disabled Mode User ({_currentUserGuid})" : "System User",
            Roles = GetCurrentUserRoles().ToList(),
            IsAuthenticated = IsAuthenticated()
        };
    }

    /// <summary>
    /// Generates service JWT token with current user GUID context
    /// </summary>
    public async Task<string?> GetCurrentJwtTokenAsync()
    {
        if (string.IsNullOrEmpty(_currentUserGuid))
        {
            _logger.LogWarning("No user GUID context set - cannot generate service JWT");
            return null;
        }

        if (!Guid.TryParse(_currentUserGuid, out var userGuid))
        {
            _logger.LogError("Invalid GUID format in context: {UserGuid}", _currentUserGuid);
            return null;
        }

        try
        {
            var serviceJwt = await _serviceJwtService.GenerateServiceTokenAsync(
                userGuid, 
                FabrikamContracts.DTOs.AuthenticationMode.Disabled);

            _logger.LogDebug("Generated service JWT for user GUID: {UserGuid}", _currentUserGuid);
            return serviceJwt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate service JWT for user GUID: {UserGuid}", _currentUserGuid);
            return null;
        }
    }

    /// <summary>
    /// Synchronous wrapper for GetCurrentJwtTokenAsync (for legacy compatibility)
    /// </summary>
    public string? GetCurrentJwtToken()
    {
        return GetCurrentJwtTokenAsync().GetAwaiter().GetResult();
    }
}
