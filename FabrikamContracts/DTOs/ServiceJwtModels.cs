using System.ComponentModel.DataAnnotations;

namespace FabrikamContracts.DTOs;

// NOTE: ServiceJwtSettings moved to AuthenticationModels.cs for consolidation

/// <summary>
/// Request for service JWT token
/// </summary>
public class ServiceJwtRequest
{
    /// <summary>
    /// User GUID for correlation
    /// </summary>
    [Required]
    public Guid UserGuid { get; set; }

    /// <summary>
    /// Service name requesting the JWT
    /// </summary>
    [Required, StringLength(100)]
    public string ServiceName { get; set; } = "";

    /// <summary>
    /// Authentication mode context
    /// </summary>
    public AuthenticationMode AuthenticationMode { get; set; }

    /// <summary>
    /// Optional user context information
    /// </summary>
    public string? UserContext { get; set; }

    /// <summary>
    /// Optional session ID
    /// </summary>
    public string? SessionId { get; set; }
}

/// <summary>
/// Response containing service JWT token
/// </summary>
public class ServiceJwtResponse
{
    /// <summary>
    /// User GUID for correlation
    /// </summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// Generated JWT token for service authentication
    /// </summary>
    public string Token { get; set; } = "";

    /// <summary>
    /// Service JWT token for authentication
    /// </summary>
    public string ServiceJwt { get; set; } = "";

    /// <summary>
    /// Authentication mode used
    /// </summary>
    public AuthenticationMode AuthenticationMode { get; set; }

    /// <summary>
    /// Token expiration timestamp
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Instructions for using the token
    /// </summary>
    public string Instructions { get; set; } = "";
}

/// <summary>
/// User details response for validation
/// </summary>
public class UserDetailsResponse
{
    /// <summary>
    /// User GUID
    /// </summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// User email
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// User organization
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Authentication mode
    /// </summary>
    public AuthenticationMode AuthenticationMode { get; set; }

    /// <summary>
    /// Registration date
    /// </summary>
    public DateTime RegistrationDate { get; set; }

    /// <summary>
    /// Whether user is valid/active
    /// </summary>
    public bool IsValid { get; set; }
}

/// <summary>
/// GUID validation response
/// </summary>
public class GuidValidationResponse
{
    /// <summary>
    /// User GUID being validated
    /// </summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// Whether the GUID is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation message
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// User details if GUID is valid
    /// </summary>
    public UserDetailsResponse? UserDetails { get; set; }

    /// <summary>
    /// Validation timestamp
    /// </summary>
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Validation configuration response
/// </summary>
public class ValidationConfigResponse
{
    /// <summary>
    /// Whether validation is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether database validation is enabled
    /// </summary>
    public bool DatabaseValidation { get; set; }

    /// <summary>
    /// Cache duration in minutes
    /// </summary>
    public int CacheMinutes { get; set; }

    /// <summary>
    /// Validation rules
    /// </summary>
    public string[] Rules { get; set; } = Array.Empty<string>();
}
