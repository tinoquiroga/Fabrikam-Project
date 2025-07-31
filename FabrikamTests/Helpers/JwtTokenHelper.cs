using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FabrikamTests.Helpers;

/// <summary>
/// Helper class for generating JWT tokens for testing purposes
/// Provides various token types and configurations for comprehensive testing scenarios
/// </summary>
public static class JwtTokenHelper
{
    /// <summary>
    /// Test secret key - only for testing, never use in production
    /// </summary>
    private const string TestSecretKey = "TestSecretKeyForFabrikamAuthenticationTestsOnly_MustBeAtLeast32Characters!";
    
    /// <summary>
    /// Test issuer for JWT tokens
    /// </summary>
    private const string TestIssuer = "FabrikamTestIssuer";
    
    /// <summary>
    /// Test audience for JWT tokens
    /// </summary>
    private const string TestAudience = "FabrikamTestAudience";

    /// <summary>
    /// Gets the signing key used for test JWT tokens
    /// </summary>
    public static SymmetricSecurityKey GetTestSigningKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
    }

    /// <summary>
    /// Gets the token validation parameters for test scenarios
    /// </summary>
    public static TokenValidationParameters GetTestTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TestIssuer,
            ValidateAudience = true,
            ValidAudience = TestAudience,
            ValidateLifetime = true,
            IssuerSigningKey = GetTestSigningKey(),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
        };
    }

    /// <summary>
    /// Generates a valid JWT token for testing API endpoints
    /// </summary>
    /// <param name="userId">User ID to include in token claims</param>
    /// <param name="username">Username to include in token claims</param>
    /// <param name="roles">Roles to include in token claims</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes (default: 60)</param>
    /// <returns>JWT token string</returns>
    public static string GenerateTestToken(
        string userId = "lee.gu@fabrikam.levelupcsp.com",
        string username = "lee.gu@fabrikam.levelupcsp.com",
        string[]? roles = null,
        int expiresInMinutes = 60)
    {
        roles ??= new[] { "User" };

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, username),
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = GetTestSigningKey();
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates an admin JWT token for testing admin-level endpoints
    /// </summary>
    /// <param name="userId">Admin user ID</param>
    /// <param name="username">Admin username</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes</param>
    /// <returns>JWT token string with admin role</returns>
    public static string GenerateAdminToken(
        string userId = "admin-user-456",
        string username = "admin@fabrikam.com",
        int expiresInMinutes = 60)
    {
        return GenerateTestToken(userId, username, new[] { "Admin", "User" }, expiresInMinutes);
    }

    /// <summary>
    /// Generates a manager JWT token for testing manager-level endpoints
    /// </summary>
    /// <param name="userId">Manager user ID</param>
    /// <param name="username">Manager username</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes</param>
    /// <returns>JWT token string with manager role</returns>
    public static string GenerateManagerToken(
        string userId = "manager-user-789",
        string username = "manager@fabrikam.com",
        int expiresInMinutes = 60)
    {
        return GenerateTestToken(userId, username, new[] { "Manager", "User" }, expiresInMinutes);
    }

    /// <summary>
    /// Generates an expired JWT token for testing authentication failure scenarios
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="username">Username</param>
    /// <returns>Expired JWT token string</returns>
    public static string GenerateExpiredToken(
        string userId = "expired-user-999",
        string username = "expired@fabrikam.com")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, username),
            new(ClaimTypes.Role, "User")
        };

        var key = GetTestSigningKey();
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-10), // Expired 10 minutes ago
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates an invalid JWT token for testing authentication failure scenarios
    /// </summary>
    /// <returns>Invalid JWT token string</returns>
    public static string GenerateInvalidToken()
    {
        return "invalid.jwt.token";
    }

    /// <summary>
    /// Generates a JWT token with custom claims for specific testing scenarios
    /// </summary>
    /// <param name="customClaims">Custom claims to include in the token</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes</param>
    /// <returns>JWT token string with custom claims</returns>
    public static string GenerateTokenWithCustomClaims(
        Dictionary<string, string> customClaims,
        int expiresInMinutes = 60)
    {
        var claims = new List<Claim>();

        foreach (var claim in customClaims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        var key = GetTestSigningKey();
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
