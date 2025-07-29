using System.ComponentModel.DataAnnotations;

namespace FabrikamContracts.DTOs;

/// <summary>
/// Authentication configuration settings
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// Authentication mode for the application
    /// </summary>
    public AuthenticationMode Mode { get; set; } = AuthenticationMode.Disabled;

    /// <summary>
    /// JWT authentication settings
    /// </summary>
    public JwtSettings Jwt { get; set; } = new();

    /// <summary>
    /// Service JWT settings for MCP authentication
    /// </summary>
    public ServiceJwtSettings ServiceJwt { get; set; } = new();

    /// <summary>
    /// Entra External ID settings
    /// </summary>
    public EntraSettings Entra { get; set; } = new();

    /// <summary>
    /// GUID validation settings
    /// </summary>
    public GuidValidationSettings GuidValidation { get; set; } = new();
}

/// <summary>
/// JWT authentication settings
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// JWT secret key for signing tokens
    /// </summary>
    public string SecretKey { get; set; } = "";

    /// <summary>
    /// JWT issuer
    /// </summary>
    public string Issuer { get; set; } = "FabrikamApi";

    /// <summary>
    /// JWT audience
    /// </summary>
    public string Audience { get; set; } = "FabrikamClients";

    /// <summary>
    /// Token expiration time in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Entra External ID OAuth settings
/// </summary>
public class EntraSettings
{
    /// <summary>
    /// Azure AD B2C tenant name
    /// </summary>
    public string TenantName { get; set; } = "";

    /// <summary>
    /// Client ID for the application
    /// </summary>
    public string ClientId { get; set; } = "";

    /// <summary>
    /// Client secret for the application
    /// </summary>
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// Redirect URI for OAuth flow
    /// </summary>
    public string RedirectUri { get; set; } = "";

    /// <summary>
    /// OAuth scopes to request
    /// </summary>
    public string[] Scopes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// GUID validation settings
/// </summary>
public class GuidValidationSettings
{
    /// <summary>
    /// Require GUID validation for all requests
    /// </summary>
    public bool RequireValidation { get; set; } = false;

    /// <summary>
    /// Allow empty/null GUIDs
    /// </summary>
    public bool AllowEmpty { get; set; } = true;

    /// <summary>
    /// Validate Microsoft GUID format
    /// </summary>
    public bool ValidateMicrosoftGuidFormat { get; set; } = true;

    /// <summary>
    /// Validate GUID exists in database
    /// </summary>
    public bool ValidateGuidInDatabase { get; set; } = false;

    /// <summary>
    /// Validation cache duration in minutes
    /// </summary>
    public int ValidationCacheMinutes { get; set; } = 60;

    /// <summary>
    /// Microsoft GUID validation pattern
    /// </summary>
    public string MicrosoftGuidPattern { get; set; } = @"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$";

    /// <summary>
    /// Custom validation rules
    /// </summary>
    public string[] ValidationRules { get; set; } = Array.Empty<string>();
}
