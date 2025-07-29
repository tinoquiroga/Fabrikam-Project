using System.ComponentModel.DataAnnotations;

namespace FabrikamContracts.DTOs;

/// <summary>
/// Base user registration model for audit GUID correlation
/// </summary>
public abstract class BaseUserRegistration
{
    /// <summary>
    /// Microsoft GUID for privacy-preserving audit trails
    /// </summary>
    public Guid AuditGuid { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    /// <summary>
    /// User's display name
    /// </summary>
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Optional organization/company name
    /// </summary>
    [StringLength(200)]
    public string? Organization { get; set; }

    /// <summary>
    /// Registration timestamp
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Authentication mode used for this user
    /// </summary>
    public AuthenticationMode AuthenticationMode { get; set; }
}

/// <summary>
/// User registration for Disabled authentication mode (GUID tracking only)
/// </summary>
public class DisabledModeUser : BaseUserRegistration
{
    /// <summary>
    /// Primary key - same as AuditGuid for disabled mode users
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Optional workshop or demo session identifier
    /// </summary>
    [StringLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    /// Optional role for workshop scenarios (Participant, Instructor, etc.)
    /// </summary>
    [StringLength(50)]
    public string? WorkshopRole { get; set; }
}

/// <summary>
/// User registration for BearerToken authentication mode (JWT users)
/// </summary>
public class AuthenticatedUser : BaseUserRegistration
{
    /// <summary>
    /// Primary key - user ID from JWT token
    /// </summary>
    [Key]
    public string UserId { get; set; } = "";

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// User roles from JWT claims
    /// </summary>
    [StringLength(200)]
    public string? Roles { get; set; }
}

/// <summary>
/// User registration for EntraExternalId authentication mode (OAuth users)
/// </summary>
public class OAuthUser : BaseUserRegistration
{
    /// <summary>
    /// Primary key - Azure AD Object ID
    /// </summary>
    [Key]
    public string AzureObjectId { get; set; } = "";

    /// <summary>
    /// Azure AD Tenant ID
    /// </summary>
    [StringLength(36)]
    public string? TenantId { get; set; }

    /// <summary>
    /// Last OAuth login timestamp
    /// </summary>
    public DateTime? LastOAuthLoginDate { get; set; }

    /// <summary>
    /// OAuth scopes granted
    /// </summary>
    [StringLength(500)]
    public string? GrantedScopes { get; set; }
}

/// <summary>
/// Request model for disabled mode user registration
/// </summary>
public class RegisterDisabledUserRequest
{
    /// <summary>
    /// User's display name
    /// </summary>
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    /// <summary>
    /// User's email address
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    /// <summary>
    /// Optional organization/company name
    /// </summary>
    [StringLength(200)]
    public string? Organization { get; set; }

    /// <summary>
    /// Optional workshop or demo session identifier
    /// </summary>
    [StringLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    /// Optional role for workshop scenarios
    /// </summary>
    [StringLength(50)]
    public string? WorkshopRole { get; set; }
}

/// <summary>
/// Request model for disabled mode user registration (MCP controller variant)
/// </summary>
public class DisabledModeUserRegistrationRequest
{
    /// <summary>
    /// User's display name
    /// </summary>
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    /// <summary>
    /// User's email address
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    /// <summary>
    /// Optional organization/company name
    /// </summary>
    [StringLength(200)]
    public string? Organization { get; set; }

    /// <summary>
    /// Optional workshop or demo session identifier
    /// </summary>
    [StringLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    /// Optional role for workshop scenarios
    /// </summary>
    [StringLength(50)]
    public string? WorkshopRole { get; set; }

    /// <summary>
    /// Optional Microsoft GUID for correlation
    /// </summary>
    public Guid? UserGuid { get; set; }
}

/// <summary>
/// Response model for user registration
/// </summary>
public class UserRegistrationResponse
{
    /// <summary>
    /// Generated Microsoft GUID for audit tracking
    /// </summary>
    public Guid AuditGuid { get; set; }

    /// <summary>
    /// User GUID for correlation
    /// </summary>
    public Guid UserGuid { get; set; }

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
    /// Success message
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Instructions for using the GUID
    /// </summary>
    public string Instructions { get; set; } = "";
}

/// <summary>
/// Authentication mode enumeration
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// GUID tracking with service JWT authentication
    /// </summary>
    Disabled,

    /// <summary>
    /// JWT Bearer token authentication
    /// </summary>
    BearerToken,

    /// <summary>
    /// Entra External ID OAuth 2.0 authentication
    /// </summary>
    EntraExternalId
}
