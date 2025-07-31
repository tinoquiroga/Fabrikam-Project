using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FabrikamApi.Models.Authentication;

namespace FabrikamApi.Services.Authentication;

/// <summary>
/// No-op JWT service for when authentication is disabled
/// Provides stub implementations to satisfy dependencies without actually generating tokens
/// </summary>
public class DisabledJwtService : IJwtService
{
    /// <summary>
    /// Initialize the disabled JWT service
    /// </summary>
    public DisabledJwtService()
    {
        // No configuration needed for disabled service
    }

    /// <summary>
    /// Returns a dummy token since authentication is disabled
    /// </summary>
    public async Task<string> GenerateAccessTokenAsync(FabrikamUser user, IList<string> roles, IList<Claim> claims)
    {
        // Return a placeholder token - this should not be used in disabled mode
        await Task.CompletedTask;
        return "disabled-auth-mode-token";
    }

    /// <summary>
    /// Returns a dummy refresh token since authentication is disabled
    /// </summary>
    public string GenerateRefreshToken()
    {
        return "disabled-auth-mode-refresh-token";
    }

    /// <summary>
    /// Returns null since tokens aren't validated in disabled mode
    /// </summary>
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        // No token validation in disabled mode
        await Task.CompletedTask;
        return null;
    }

    /// <summary>
    /// Returns null since tokens aren't validated in disabled mode
    /// </summary>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        // No token validation in disabled mode
        return null;
    }

    /// <summary>
    /// Returns a dummy expiration since authentication is disabled
    /// </summary>
    public DateTime GetTokenExpiration()
    {
        // Return a far future date since tokens aren't used
        return DateTime.UtcNow.AddYears(10);
    }
}
