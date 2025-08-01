using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FabrikamApi.Models.Authentication;
using Microsoft.Extensions.Options;

namespace FabrikamApi.Services.Authentication;

/// <summary>
/// Service for generating and validating JWT tokens
/// Handles token creation, refresh, and validation for ASP.NET Identity authentication
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT access token for the authenticated user
    /// </summary>
    /// <param name="user">The authenticated user</param>
    /// <param name="roles">User's roles</param>
    /// <param name="claims">Additional user claims</param>
    /// <returns>JWT token string</returns>
    Task<string> GenerateAccessTokenAsync(FabrikamUser user, IList<string> roles, IList<Claim> claims);

    /// <summary>
    /// Generates a refresh token for token renewal
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns the principal
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);

    /// <summary>
    /// Gets the principal from an expired token (for refresh scenarios)
    /// </summary>
    /// <param name="token">Expired JWT token</param>
    /// <returns>ClaimsPrincipal if format is valid</returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Gets the expiration date for tokens
    /// </summary>
    /// <returns>Token expiration DateTime</returns>
    DateTime GetTokenExpiration();
}

/// <summary>
/// JWT service implementation with Azure Key Vault integration
/// Follows Azure security best practices for token management
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;

        // Validate JWT configuration
        if (string.IsNullOrEmpty(_jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured. Please check your configuration.");
        }

        if (_jwtSettings.SecretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long for security.");
        }
    }

    /// <inheritdoc />
    public async Task<string> GenerateAccessTokenAsync(FabrikamUser user, IList<string> roles, IList<Claim> claims)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            // Build claims list
            var tokenClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new("firstName", user.FirstName),
                new("lastName", user.LastName),
                new("displayName", user.DisplayName),
                new("isActive", user.IsActive.ToString()),
                new("isAdmin", user.IsAdmin.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add customer ID if available
            if (user.CustomerId.HasValue)
            {
                tokenClaims.Add(new Claim("customerId", user.CustomerId.Value.ToString()));
            }

            // Add roles
            foreach (var role in roles)
            {
                tokenClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add additional claims
            tokenClaims.AddRange(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(tokenClaims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("Generated JWT token for user {UserId} with {RoleCount} roles and {ClaimCount} claims",
                user.Id, roles.Count, claims.Count);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        try
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);

            _logger.LogDebug("Generated refresh token");
            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refresh token");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = _jwtSettings.ValidateLifetime,
                ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewInMinutes)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogDebug("Successfully validated JWT token");
                return principal;
            }

            _logger.LogWarning("JWT token validation failed: Invalid algorithm");
            return null;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("JWT token validation failed: Token expired");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating JWT token");
            return null;
        }
    }

    /// <inheritdoc />
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false, // Don't validate lifetime for expired tokens
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token format for expired token validation");
                return null;
            }

            _logger.LogDebug("Successfully extracted principal from expired token");
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting principal from expired token");
            return null;
        }
    }

    /// <inheritdoc />
    public DateTime GetTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
    }
}

/// <summary>
/// Null implementation of IJwtService for Disabled authentication mode
/// </summary>
public class NullJwtService : IJwtService
{
    public Task<string> GenerateAccessTokenAsync(FabrikamUser user, IList<string> roles, IList<Claim> claims)
    {
        throw new InvalidOperationException("JWT tokens are not available in Disabled authentication mode");
    }

    public string GenerateRefreshToken()
    {
        throw new InvalidOperationException("JWT tokens are not available in Disabled authentication mode");
    }

    public Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        throw new InvalidOperationException("JWT tokens are not available in Disabled authentication mode");
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        throw new InvalidOperationException("JWT tokens are not available in Disabled authentication mode");
    }

    public DateTime GetTokenExpiration()
    {
        throw new InvalidOperationException("JWT tokens are not available in Disabled authentication mode");
    }
}
