namespace FabrikamMcp.Models;

/// <summary>
/// JWT configuration settings for MCP server authentication
/// Maps to appsettings configuration sections
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name for JWT settings
    /// </summary>
    public const string SectionName = "Authentication:Jwt";

    /// <summary>
    /// Secret key for validating JWT tokens (must match API server)
    /// Should be stored in Azure Key Vault or User Secrets
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT token issuer (must match API server issuer)
    /// </summary>
    public string Issuer { get; set; } = "https://localhost:7297";

    /// <summary>
    /// JWT token audience (must match API server audience)
    /// </summary>
    public string Audience { get; set; } = "FabrikamApi";

    /// <summary>
    /// Whether to validate the token issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate the token audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Whether to validate the signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance in minutes for token validation
    /// </summary>
    public int ClockSkewInMinutes { get; set; } = 5;

    /// <summary>
    /// Whether to require HTTPS for token validation
    /// Should be true in production
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = false;
}

/// <summary>
/// Authentication settings for MCP server
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// Configuration section name for authentication settings
    /// </summary>
    public const string SectionName = "Authentication";

    /// <summary>
    /// JWT token configuration
    /// </summary>
    public JwtSettings Jwt { get; set; } = new();

    /// <summary>
    /// Whether authentication is required for MCP tools
    /// </summary>
    public bool RequireAuthentication { get; set; } = true;
}
