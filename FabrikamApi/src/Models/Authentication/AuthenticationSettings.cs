namespace FabrikamApi.Models.Authentication;

/// <summary>
/// JWT configuration settings for ASP.NET Identity authentication
/// Maps to appsettings configuration sections
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name for JWT settings
    /// </summary>
    public const string SectionName = "Authentication:AspNetIdentity:Jwt";

    /// <summary>
    /// Secret key for signing JWT tokens
    /// Should be stored in Azure Key Vault or User Secrets
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT token issuer (typically the API URL)
    /// </summary>
    public string Issuer { get; set; } = "https://localhost:7297";

    /// <summary>
    /// JWT token audience (client application identifier)
    /// </summary>
    public string Audience { get; set; } = "FabrikamApi";

    /// <summary>
    /// Token expiration time in minutes (default: 60 minutes)
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token expiration time in days (default: 7 days)
    /// </summary>
    public int RefreshTokenExpirationInDays { get; set; } = 7;

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
}

/// <summary>
/// Authentication settings container
/// Contains all authentication-related configuration
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// Configuration section name for authentication settings
    /// </summary>
    public const string SectionName = "Authentication";

    /// <summary>
    /// ASP.NET Identity specific settings
    /// </summary>
    public AspNetIdentitySettings AspNetIdentity { get; set; } = new();
}

/// <summary>
/// ASP.NET Identity specific configuration
/// </summary>
public class AspNetIdentitySettings
{
    /// <summary>
    /// JWT token configuration
    /// </summary>
    public JwtSettings Jwt { get; set; } = new();

    /// <summary>
    /// Password policy configuration
    /// </summary>
    public PasswordSettings Password { get; set; } = new();

    /// <summary>
    /// User lockout configuration
    /// </summary>
    public LockoutSettings Lockout { get; set; } = new();

    /// <summary>
    /// Email confirmation requirements
    /// </summary>
    public bool RequireConfirmedEmail { get; set; } = false;

    /// <summary>
    /// Phone number confirmation requirements
    /// </summary>
    public bool RequireConfirmedPhoneNumber { get; set; } = false;
}

/// <summary>
/// Password policy configuration
/// Defines security requirements for user passwords
/// </summary>
public class PasswordSettings
{
    /// <summary>
    /// Minimum password length
    /// </summary>
    public int RequiredLength { get; set; } = 8;

    /// <summary>
    /// Require at least one uppercase letter
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Require at least one lowercase letter
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Require at least one digit
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// Require at least one non-alphanumeric character
    /// </summary>
    public bool RequireNonAlphanumeric { get; set; } = true;

    /// <summary>
    /// Number of unique characters required
    /// </summary>
    public int RequiredUniqueChars { get; set; } = 1;
}

/// <summary>
/// User lockout configuration
/// Defines account lockout behavior for security
/// </summary>
public class LockoutSettings
{
    /// <summary>
    /// Default lockout time span in minutes
    /// </summary>
    public int DefaultLockoutTimeSpanInMinutes { get; set; } = 15;

    /// <summary>
    /// Maximum number of failed access attempts before lockout
    /// </summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;

    /// <summary>
    /// Whether new users are locked out by default
    /// </summary>
    public bool AllowedForNewUsers { get; set; } = true;
}
