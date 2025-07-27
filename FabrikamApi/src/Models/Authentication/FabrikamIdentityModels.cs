using Microsoft.AspNetCore.Identity;

namespace FabrikamApi.Models.Authentication;

/// <summary>
/// Custom application user extending ASP.NET Core Identity
/// Integrates with Fabrikam business entities and customer data
/// </summary>
public class FabrikamUser : IdentityUser
{
    /// <summary>
    /// User's first name for personalization
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name for personalization
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Microsoft Entra External ID ObjectId for external identity integration
    /// Links local user data to Entra External ID identity
    /// </summary>
    public string? EntraObjectId { get; set; }

    /// <summary>
    /// Company/Organization name for business context
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Date when the user account was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the user account was last updated
    /// </summary>
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the user was soft-deleted (null if active)
    /// Enables GDPR-compliant soft delete functionality
    /// </summary>
    public DateTime? DeletedDate { get; set; }

    /// <summary>
    /// Last time the user was active in the system
    /// </summary>
    public DateTime? LastActiveDate { get; set; }

    /// <summary>
    /// Whether the user account is currently active
    /// Allows for soft-disable without deletion
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional reference to Customer entity for business users
    /// Links authentication user to business customer data
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Navigation property to associated customer (if applicable)
    /// Enables business data integration
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// User's preferred notification settings
    /// </summary>
    public string NotificationPreferences { get; set; } = "email";

    /// <summary>
    /// Display name for UI personalization
    /// </summary>
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Whether this user has admin privileges
    /// Helper property for role-based access
    /// </summary>
    public bool IsAdmin { get; set; } = false;
}

/// <summary>
/// Custom application role for Fabrikam business operations
/// Extends ASP.NET Core Identity roles with business context
/// </summary>
public class FabrikamRole : IdentityRole
{
    /// <summary>
    /// Human-readable description of the role
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this role is active and can be assigned
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the role was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Priority level for role hierarchy (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;
}

/// <summary>
/// Custom UserRole entity with audit tracking for role assignments
/// Extends standard ASP.NET Identity UserRole with business audit requirements
/// </summary>
public class FabrikamUserRole : IdentityUserRole<string>
{
    /// <summary>
    /// Timestamp when the role was assigned to the user
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who assigned this role (for audit trail)
    /// </summary>
    public string? AssignedBy { get; set; }

    /// <summary>
    /// Optional notes about the role assignment
    /// </summary>
    public string? AssignmentNotes { get; set; }

    /// <summary>
    /// Whether this role assignment is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the role assignment expires (null = no expiration)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Extended user claim for Fabrikam-specific permissions
/// Stores custom business logic permissions
/// </summary>
public class FabrikamUserClaim : IdentityUserClaim<string>
{
    /// <summary>
    /// Date when this claim was assigned
    /// </summary>
    public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who granted this claim (for audit trail)
    /// </summary>
    public string? GrantedBy { get; set; }

    /// <summary>
    /// Optional expiration date for time-limited permissions
    /// </summary>
    public DateTime? ExpiresDate { get; set; }

    /// <summary>
    /// Whether this claim is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
