using FabrikamContracts.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FabrikamMcp.Services;

/// <summary>
/// Service for generating and validating JWT tokens for service-to-service communication
/// </summary>
public interface IServiceJwtService
{
    /// <summary>
    /// Generate a service JWT token with user GUID context
    /// </summary>
    Task<string> GenerateServiceTokenAsync(Guid userGuid, AuthenticationMode mode, string? sessionId = null);

    /// <summary>
    /// Validate a service JWT token
    /// </summary>
    Task<ClaimsPrincipal?> ValidateServiceTokenAsync(string token);

    /// <summary>
    /// Extract user GUID from service JWT token
    /// </summary>
    Task<Guid?> ExtractUserGuidFromTokenAsync(string token);
}

/// <summary>
/// Implementation of service JWT generation and validation
/// </summary>
public class ServiceJwtService : IServiceJwtService
{
    private readonly ServiceJwtSettings _jwtSettings;
    private readonly IUserRegistrationService _userRegistrationService;
    private readonly ILogger<ServiceJwtService> _logger;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public ServiceJwtService(
        IOptions<AuthenticationSettings> authSettings,
        IUserRegistrationService userRegistrationService,
        ILogger<ServiceJwtService> logger)
    {
        _jwtSettings = authSettings.Value.ServiceJwt;
        _userRegistrationService = userRegistrationService;
        _logger = logger;

        // Configure token validation parameters
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    }

    /// <summary>
    /// Generate a service JWT token with user GUID context
    /// </summary>
    public async Task<string> GenerateServiceTokenAsync(Guid userGuid, AuthenticationMode mode, string? sessionId = null)
    {
        if (userGuid == Guid.Empty)
            throw new ArgumentException("Valid user GUID is required", nameof(userGuid));

        // Validate the user GUID exists in our system
        var userExists = await _userRegistrationService.ValidateUserGuidAsync(userGuid);
        if (!userExists)
            throw new InvalidOperationException($"User GUID {userGuid} not found in registry");

        // Get user details for claims
        var user = await _userRegistrationService.GetUserByGuidAsync(userGuid);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userGuid.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
            new Claim(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),
            new Claim("fabrikam_auth_mode", mode.ToString()),
            new Claim("fabrikam_service_jwt", "true")
        };

        // Add user-specific claims if available
        if (user != null)
        {
            if (!string.IsNullOrEmpty(user.Name))
                claims.Add(new Claim("name", user.Name));
            
            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim("email", user.Email));
            
            claims.Add(new Claim("auth_mode", user.AuthenticationMode.ToString()));
        }

        // Add session ID if provided
        if (!string.IsNullOrEmpty(sessionId))
            claims.Add(new Claim("fabrikam_session_id", sessionId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogDebug("Generated service JWT for user {UserGuid} in {AuthMode} mode", 
            userGuid, mode);
        
        return tokenString;
    }

    /// <summary>
    /// Validate a service JWT token
    /// </summary>
    public async Task<ClaimsPrincipal?> ValidateServiceTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            
            // Ensure this is a service JWT
            var isServiceJwt = principal.FindFirst("fabrikam_service_jwt")?.Value == "true";
            if (!isServiceJwt)
            {
                _logger.LogWarning("Token is not a valid Fabrikam service JWT");
                return null;
            }

            // Validate the user GUID still exists in our system
            var userGuidClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
            if (Guid.TryParse(userGuidClaim, out var userGuid))
            {
                var userExists = await _userRegistrationService.ValidateUserGuidAsync(userGuid);
                if (!userExists)
                {
                    _logger.LogWarning("User GUID {UserGuid} in JWT no longer exists in registry", userGuid);
                    return null;
                }
            }
            else
            {
                _logger.LogWarning("Invalid user GUID format in service JWT");
                return null;
            }

            return principal;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning("Service JWT validation failed: {Error}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating service JWT");
            return null;
        }
    }

    /// <summary>
    /// Extract user GUID from service JWT token
    /// </summary>
    public async Task<Guid?> ExtractUserGuidFromTokenAsync(string token)
    {
        var principal = await ValidateServiceTokenAsync(token);
        
        if (principal == null)
            return null;

        var userGuidClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        
        return Guid.TryParse(userGuidClaim, out var userGuid) ? userGuid : null;
    }
}
