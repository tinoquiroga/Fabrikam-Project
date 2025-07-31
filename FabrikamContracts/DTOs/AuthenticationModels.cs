using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FabrikamContracts.DTOs;

/// <summary>
/// Complete authentication configuration for MCP server
/// Covers end-user authentication modes (Disabled/JWT/EntraExternalID)
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// Configuration section name for authentication settings
    /// </summary>
    public const string SectionName = "Authentication";

    /// <summary>
    /// Primary authentication mode for end users accessing MCP endpoint
    /// Defaults to BearerToken for production security, but can be overridden for testing
    /// </summary>
    public AuthenticationMode Mode { get; set; } = GetDefaultAuthenticationMode();

    /// <summary>
    /// Gets the default authentication mode based on environment
    /// Uses Disabled for testing environments, BearerToken for production
    /// </summary>
    private static AuthenticationMode GetDefaultAuthenticationMode()
    {
        // Check if we're in a testing environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
                         Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? 
                         "Production";
        
        // For testing environments, default to Disabled for easier test setup
        if (environment.Equals("Test", StringComparison.OrdinalIgnoreCase) ||
            environment.Equals("Testing", StringComparison.OrdinalIgnoreCase) ||
            environment.Contains("Test"))
        {
            return AuthenticationMode.Disabled;
        }
        
        // For all other environments (Production, Development, etc.), default to secure mode
        return AuthenticationMode.BearerToken;
    }

    /// <summary>
    /// JWT settings for Bearer token authentication (when Mode = BearerToken)
    /// </summary>
    public JwtSettings Jwt { get; set; } = new();

    /// <summary>
    /// Service JWT configuration for MCP-to-API communication (always enabled)
    /// </summary>
    public ServiceJwtSettings ServiceJwt { get; set; } = new();

    /// <summary>
    /// GUID validation settings for Disabled authentication mode
    /// </summary>
    public GuidValidationSettings GuidValidation { get; set; } = new();

    /// <summary>
    /// Entra External ID settings (when Mode = EntraExternalId)
    /// </summary>
    public EntraExternalIdSettings? EntraExternalId { get; set; }

    /// <summary>
    /// Whether to enable comprehensive audit logging
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Whether user authentication is required based on current mode
    /// </summary>
    public bool RequireUserAuthentication => Mode != AuthenticationMode.Disabled;

    /// <summary>
    /// Service authentication is always required for API access
    /// </summary>
    public bool RequireServiceAuthentication => true;

    /// <summary>
    /// Validates the authentication configuration
    /// </summary>
    public void Validate()
    {
        switch (Mode)
        {
            case AuthenticationMode.BearerToken:
                if (string.IsNullOrWhiteSpace(Jwt.SecretKey))
                    throw new InvalidOperationException("JWT SecretKey is required for BearerToken mode");
                Jwt.Validate();
                break;
            
            case AuthenticationMode.EntraExternalId:
                if (EntraExternalId == null)
                    throw new InvalidOperationException("EntraExternalId settings are required for EntraExternalId mode");
                EntraExternalId.Validate();
                break;
            
            case AuthenticationMode.Disabled:
                GuidValidation.Validate();
                break;
        }

        ServiceJwt.Validate();
    }
}

/// <summary>
/// JWT configuration for end-user Bearer token authentication
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name for JWT settings
    /// </summary>
    public const string SectionName = "Authentication:Jwt";

    /// <summary>
    /// JWT secret key for token signing/validation
    /// Should be loaded from secure configuration (Key Vault, environment variables)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT token issuer
    /// </summary>
    public string Issuer { get; set; } = "https://localhost:7297";

    /// <summary>
    /// JWT token audience
    /// </summary>
    public string Audience { get; set; } = "FabrikamApi";

    /// <summary>
    /// Clock skew tolerance in minutes for token validation
    /// </summary>
    public int ClockSkewInMinutes { get; set; } = 5;

    /// <summary>
    /// Token expiration in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

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
    /// Whether to require HTTPS for token validation
    /// Should be true in production
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = false;

    /// <summary>
    /// Validates JWT settings for security compliance
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new ArgumentException("SecretKey cannot be empty", nameof(SecretKey));
        
        if (SecretKey.Length < 32)
            throw new ArgumentException("SecretKey must be at least 32 characters for security", nameof(SecretKey));
        
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new ArgumentException("Issuer cannot be empty", nameof(Issuer));
        
        if (string.IsNullOrWhiteSpace(Audience))
            throw new ArgumentException("Audience cannot be empty", nameof(Audience));
        
        if (ExpirationMinutes < 1 || ExpirationMinutes > 1440) // Max 24 hours
            throw new ArgumentOutOfRangeException(nameof(ExpirationMinutes), "Must be between 1 and 1440 minutes");
        
        if (ClockSkewInMinutes < 0 || ClockSkewInMinutes > 60)
            throw new ArgumentOutOfRangeException(nameof(ClockSkewInMinutes), "Must be between 0 and 60 minutes");
    }
}

/// <summary>
/// Service JWT settings for MCP-to-API authentication (always enabled)
/// </summary>
public class ServiceJwtSettings
{
    /// <summary>
    /// Service name for JWT audience
    /// </summary>
    public string ServiceName { get; set; } = "FabrikamMcp";

    /// <summary>
    /// Service JWT secret key (different from user JWT)
    /// Should be loaded from secure configuration in production
    /// </summary>
    public string SecretKey { get; set; } = "";

    /// <summary>
    /// JWT issuer for service tokens
    /// </summary>
    public string Issuer { get; set; } = "FabrikamMcp";

    /// <summary>
    /// JWT audience for service tokens
    /// </summary>
    public string Audience { get; set; } = "FabrikamServices";

    /// <summary>
    /// Service token expiration in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 1440; // 24 hours

    /// <summary>
    /// Service token expiration in hours (computed property for backward compatibility)
    /// </summary>
    public int ExpirationHours 
    { 
        get => ExpirationMinutes / 60; 
        set => ExpirationMinutes = value * 60; 
    }

    /// <summary>
    /// Service identity for claims
    /// </summary>
    public string ServiceIdentity { get; set; } = "fabrikam-mcp-service";

    /// <summary>
    /// Whether to cache service JWTs for performance
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache refresh threshold in minutes before expiration
    /// </summary>
    public int CacheRefreshThresholdMinutes { get; set; } = 60;

    /// <summary>
    /// Allowed authentication modes for service JWT (for backward compatibility)
    /// </summary>
    public AuthenticationMode[] AllowedModes { get; set; } = { AuthenticationMode.Disabled };

    /// <summary>
    /// Validates service JWT settings
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new ArgumentException("SecretKey cannot be empty", nameof(SecretKey));
        
        if (SecretKey.Length < 32)
            throw new ArgumentException("SecretKey must be at least 32 characters", nameof(SecretKey));
        
        if (string.IsNullOrWhiteSpace(ServiceIdentity))
            throw new ArgumentException("ServiceIdentity cannot be empty", nameof(ServiceIdentity));
        
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new ArgumentException("Issuer cannot be empty", nameof(Issuer));
        
        if (string.IsNullOrWhiteSpace(Audience))
            throw new ArgumentException("Audience cannot be empty", nameof(Audience));
        
        if (ExpirationMinutes < 1 || ExpirationMinutes > 10080) // Max 1 week
            throw new ArgumentOutOfRangeException(nameof(ExpirationMinutes), "Must be between 1 and 10080 minutes");
        
        if (CacheRefreshThresholdMinutes < 1)
            throw new ArgumentOutOfRangeException(nameof(CacheRefreshThresholdMinutes), "Must be at least 1 minute");
    }
}

/// <summary>
/// GUID validation settings for Disabled authentication mode
/// </summary>
public class GuidValidationSettings
{
    /// <summary>
    /// Whether GUID validation is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether to validate Microsoft GUID format (backward compatibility)
    /// </summary>
    public bool ValidateMicrosoftGuidFormat { get; set; } = true;

    /// <summary>
    /// Whether to reject empty GUIDs
    /// </summary>
    public bool RejectEmptyGuid { get; set; } = true;

    /// <summary>
    /// Whether to validate GUIDs against database records
    /// </summary>
    public bool ValidateInDatabase { get; set; } = true;

    /// <summary>
    /// Whether to validate GUID exists in database (alias for ValidateInDatabase)
    /// </summary>
    public bool ValidateGuidInDatabase 
    { 
        get => ValidateInDatabase; 
        set => ValidateInDatabase = value; 
    }

    /// <summary>
    /// Validation result cache duration in minutes
    /// </summary>
    public int ValidationCacheMinutes { get; set; } = 60;

    /// <summary>
    /// Microsoft GUID format pattern (standard GUID format with hyphens)
    /// </summary>
    public string MicrosoftGuidPattern { get; set; } = "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

    /// <summary>
    /// GUID pattern (alias for MicrosoftGuidPattern for backward compatibility)
    /// </summary>
    public string GuidPattern 
    { 
        get => MicrosoftGuidPattern; 
        set => MicrosoftGuidPattern = value; 
    }

    /// <summary>
    /// Validation rules for configuration display
    /// </summary>
    public string[] ValidationRules { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets computed validation rules for display
    /// </summary>
    public string[] GetValidationRules() => new[]
    {
        $"GUID Format Validation: {(ValidateMicrosoftGuidFormat ? "Enabled" : "Disabled")}",
        $"Empty GUID Rejection: {(RejectEmptyGuid ? "Enabled" : "Disabled")}",
        $"Database Validation: {(ValidateInDatabase ? "Enabled" : "Disabled")}",
        $"Cache Duration: {ValidationCacheMinutes} minutes"
    };

    /// <summary>
    /// Validates GUID validation settings
    /// </summary>
    public void Validate()
    {
        if (ValidationCacheMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(ValidationCacheMinutes), "Must be non-negative");
        
        if (string.IsNullOrWhiteSpace(MicrosoftGuidPattern))
            throw new ArgumentException("MicrosoftGuidPattern cannot be empty", nameof(MicrosoftGuidPattern));
    }
}

/// <summary>
/// Entra External ID OAuth 2.0 settings
/// </summary>
public class EntraExternalIdSettings
{
    /// <summary>
    /// Entra External ID tenant ID
    /// </summary>
    public string TenantId { get; set; } = "";

    /// <summary>
    /// Application (client) ID
    /// </summary>
    public string ClientId { get; set; } = "";

    /// <summary>
    /// Client secret (should be in secure configuration)
    /// </summary>
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// OAuth 2.0 authority URL
    /// </summary>
    public string Authority { get; set; } = "";

    /// <summary>
    /// OAuth 2.0 scopes
    /// </summary>
    public string[] Scopes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Validates Entra External ID settings
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(TenantId));
        
        if (string.IsNullOrWhiteSpace(ClientId))
            throw new ArgumentException("ClientId cannot be empty", nameof(ClientId));
        
        if (string.IsNullOrWhiteSpace(ClientSecret))
            throw new ArgumentException("ClientSecret cannot be empty", nameof(ClientSecret));
        
        if (string.IsNullOrWhiteSpace(Authority))
            throw new ArgumentException("Authority cannot be empty", nameof(Authority));
    }
}

/// <summary>
/// Authentication context for tracking user sessions
/// </summary>
public class AuthenticationContext
{
    /// <summary>
    /// User GUID for correlation
    /// </summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// Authentication mode used
    /// </summary>
    public AuthenticationMode AuthenticationMode { get; set; }

    /// <summary>
    /// User display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// User name (alias for DisplayName for backward compatibility)
    /// </summary>
    public string? UserName 
    { 
        get => DisplayName; 
        set => DisplayName = value; 
    }

    /// <summary>
    /// User ID (can be GUID string or any string identifier for backward compatibility)
    /// </summary>
    private string? _userId;
    public string? UserId 
    { 
        get => _userId ?? (UserGuid == Guid.Empty ? null : UserGuid.ToString()); 
        set 
        {
            _userId = value;
            if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out var guid))
                UserGuid = guid;
        }
    }

    /// <summary>
    /// User email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User roles/permissions
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Session ID for tracking
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Session start timestamp
    /// </summary>
    public DateTime SessionStart { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether user is authenticated (for backward compatibility)
    /// </summary>
    public bool IsAuthenticated { get; set; } = false;

    /// <summary>
    /// Checks if user has specific role (case-insensitive)
    /// </summary>
    public bool HasRole(string role) =>
        Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if user has any of the specified roles (case-insensitive)
    /// </summary>
    public bool HasAnyRole(params string[] roles) =>
        roles?.Any(HasRole) ?? false;

    /// <summary>
    /// Gets display name with fallback logic matching test expectations
    /// </summary>
    public string GetDisplayName()
    {
        if (!IsAuthenticated)
            return "Anonymous";
            
        if (!string.IsNullOrWhiteSpace(DisplayName))
            return DisplayName;
            
        if (!string.IsNullOrWhiteSpace(UserId))
            return UserId;
            
        return "Unknown User";
    }
}
