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
/// Authentication mode enumeration for MCP server
/// Enhanced security-by-default approach: service-to-service always secured
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// GUID tracking with service JWT authentication
    /// User provides GUID, MCP generates service JWT for API calls
    /// </summary>
    Disabled,

    /// <summary>
    /// JWT Bearer token authentication (standard for ASP.NET Core Identity)
    /// User provides JWT, MCP forwards JWT for API calls
    /// </summary>
    BearerToken,

    /// <summary>
    /// Entra External ID (Azure B2C) OAuth 2.0 authentication
    /// User provides OAuth token, MCP converts to JWT for API calls
    /// </summary>
    EntraExternalId
}

/// <summary>
/// Microsoft GUID validation settings for Disabled authentication mode
/// </summary>
public class GuidValidationSettings
{
    /// <summary>
    /// Microsoft GUID format pattern (standard GUID format with hyphens)
    /// </summary>
    public string GuidPattern { get; set; } = "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

    /// <summary>
    /// Whether to reject empty GUIDs (00000000-0000-0000-0000-000000000000)
    /// </summary>
    public bool RejectEmptyGuid { get; set; } = true;

    /// <summary>
    /// Whether to validate GUID exists in user registration database
    /// </summary>
    public bool ValidateInDatabase { get; set; } = true;

    /// <summary>
    /// Cache duration for GUID validation results in minutes
    /// </summary>
    public int ValidationCacheMinutes { get; set; } = 60;
}

/// <summary>
/// Service JWT settings for service-to-service authentication
/// Always used regardless of user authentication mode
/// </summary>
public class ServiceJwtSettings
{
    /// <summary>
    /// Service JWT expiration time in hours (default: 24)
    /// </summary>
    public int ExpirationHours { get; set; } = 24;

    /// <summary>
    /// Service identity name for JWT claims
    /// </summary>
    public string ServiceIdentity { get; set; } = "fabrikam-mcp-service";

    /// <summary>
    /// Whether to cache service JWTs (recommended: true)
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache refresh threshold in minutes before expiration
    /// </summary>
    public int CacheRefreshThresholdMinutes { get; set; } = 60;
}

/// <summary>
/// Authentication settings for MCP server
/// Enhanced security-by-default architecture
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// Configuration section name for authentication settings
    /// </summary>
    public const string SectionName = "Authentication";

    /// <summary>
    /// Authentication mode to use for user authentication
    /// </summary>
    public AuthenticationMode Mode { get; set; } = AuthenticationMode.BearerToken;

    /// <summary>
    /// JWT token configuration for user authentication
    /// </summary>
    public JwtSettings Jwt { get; set; } = new();

    /// <summary>
    /// Service JWT configuration for service-to-service authentication
    /// Always enabled for API security
    /// </summary>
    public ServiceJwtSettings ServiceJwt { get; set; } = new();

    /// <summary>
    /// GUID validation settings for Disabled authentication mode
    /// </summary>
    public GuidValidationSettings GuidValidation { get; set; } = new();

    /// <summary>
    /// Whether to enable comprehensive audit logging
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Whether user authentication is required (varies by mode)
    /// Service-to-service authentication is always required
    /// </summary>
    public bool RequireUserAuthentication => Mode != AuthenticationMode.Disabled;

    /// <summary>
    /// Whether service-to-service authentication is required (always true)
    /// </summary>
    public bool RequireServiceAuthentication => true;
}
