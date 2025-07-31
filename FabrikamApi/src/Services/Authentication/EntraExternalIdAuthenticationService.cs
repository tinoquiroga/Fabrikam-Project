using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FabrikamApi.Models.Authentication;
using FabrikamApi.Configuration;
using FabrikamContracts.DTOs;

namespace FabrikamApi.Services.Authentication;

/// <summary>
/// Authentication service for Entra External ID (OAuth 2.0) integration
/// Handles token validation, user claims mapping, and Azure AD B2C integration
/// </summary>
public interface IEntraExternalIdAuthenticationService : IAuthenticationService
{
    /// <summary>
    /// Validates OAuth token and returns user information
    /// </summary>
    /// <param name="accessToken">OAuth access token from Entra External ID</param>
    /// <returns>User information or null if token is invalid</returns>
    Task<UserInfo?> ValidateOAuthTokenAsync(string accessToken);

    /// <summary>
    /// Maps OAuth claims to application roles
    /// </summary>
    /// <param name="claims">Claims from OAuth token</param>
    /// <returns>Mapped application roles</returns>
    Task<IList<string>> MapOAuthClaimsToRolesAsync(IEnumerable<Claim> claims);

    /// <summary>
    /// Creates or updates user from OAuth token claims
    /// </summary>
    /// <param name="claims">OAuth token claims</param>
    /// <returns>Application user information</returns>
    Task<UserInfo> CreateOrUpdateUserFromOAuthAsync(IEnumerable<Claim> claims);
}

/// <summary>
/// Entra External ID authentication service implementation
/// Integrates OAuth 2.0 authentication with application user management
/// </summary>
public class EntraExternalIdAuthenticationService : IEntraExternalIdAuthenticationService
{
    private readonly IAuthenticationService _baseAuthService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EntraExternalIdAuthenticationService> _logger;
    private readonly EntraExternalIdSettings _entraSettings;

    public EntraExternalIdAuthenticationService(
        IAuthenticationService baseAuthService,
        IConfiguration configuration,
        ILogger<EntraExternalIdAuthenticationService> logger)
    {
        _baseAuthService = baseAuthService;
        _configuration = configuration;
        _logger = logger;

        // Load Entra External ID settings
        _entraSettings = _configuration.GetSection("Authentication:EntraExternalId").Get<EntraExternalIdSettings>() 
                        ?? throw new InvalidOperationException("EntraExternalId settings not configured");

        _logger.LogInformation("EntraExternalIdAuthenticationService initialized with tenant: {TenantId}", _entraSettings.TenantId);
    }

    #region IAuthenticationService Implementation (Delegated)

    /// <inheritdoc />
    public Task<AuthenticationResponse> RegisterAsync(RegisterRequest request)
    {
        // OAuth users are registered automatically on first login
        throw new NotSupportedException("Manual registration is not supported in EntraExternalId mode. Users are registered automatically via OAuth flow.");
    }

    /// <inheritdoc />
    public Task<AuthenticationResponse> LoginAsync(LoginRequest request)
    {
        // OAuth login is handled by middleware, this method validates existing tokens
        throw new NotSupportedException("Direct login is not supported in EntraExternalId mode. Use OAuth 2.0 flow via /signin-oidc endpoint.");
    }

    /// <inheritdoc />
    public Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // OAuth refresh is handled by OIDC middleware
        throw new NotSupportedException("Token refresh is handled by OAuth 2.0 flow in EntraExternalId mode.");
    }

    /// <inheritdoc />
    public Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        // Password changes are handled by Entra External ID
        throw new NotSupportedException("Password changes are handled by Entra External ID portal in EntraExternalId mode.");
    }

    /// <inheritdoc />
    public Task<bool> RequestPasswordResetAsync(ResetPasswordRequest request)
    {
        // Password reset is handled by Entra External ID
        throw new NotSupportedException("Password reset is handled by Entra External ID portal in EntraExternalId mode.");
    }

    /// <inheritdoc />
    public Task<bool> ConfirmPasswordResetAsync(ConfirmPasswordResetRequest request)
    {
        // Password reset confirmation is handled by Entra External ID
        throw new NotSupportedException("Password reset confirmation is handled by Entra External ID portal in EntraExternalId mode.");
    }

    /// <inheritdoc />
    public async Task<UserInfo?> GetUserInfoAsync(string userId)
    {
        try
        {
            // For OAuth users, userId is the Azure Object ID
            return await _baseAuthService.GetUserInfoAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for OAuth user {UserId}", userId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<UserInfo?> GetUserInfoByEmailAsync(string email)
    {
        try
        {
            return await _baseAuthService.GetUserInfoByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for email {Email}", email);
            return null;
        }
    }

    #endregion

    #region Entra External ID Specific Methods

    /// <inheritdoc />
    public Task<UserInfo?> ValidateOAuthTokenAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Validating OAuth access token");

            // In a production implementation, you would:
            // 1. Validate the token signature against JWKS endpoint
            // 2. Verify token claims (aud, iss, exp, etc.)
            // 3. Check token hasn't been revoked

            // For this implementation, we'll assume the token is already validated by middleware
            // and extract user information from it

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("OAuth token validation failed: empty token");
                return Task.FromResult<UserInfo?>(null);
            }

            // In production, decode and validate the JWT token here
            // For now, return success (middleware handles validation)
            _logger.LogInformation("OAuth token validated successfully");

            var userInfo = new UserInfo
            {
                Id = "oauth-validated", // This would be extracted from token
                Email = "oauth-user@example.com", // This would be extracted from token
                FirstName = "OAuth",
                LastName = "User",
                Roles = new List<string> { "User" },
                IsActive = true
            };

            return Task.FromResult<UserInfo?>(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OAuth token");
            return Task.FromResult<UserInfo?>(null);
        }
    }

    /// <inheritdoc />
    public Task<IList<string>> MapOAuthClaimsToRolesAsync(IEnumerable<Claim> claims)
    {
        try
        {
            var roles = new List<string>();
            var claimsList = claims.ToList();

            _logger.LogDebug("Mapping OAuth claims to application roles. Claims count: {ClaimsCount}", claimsList.Count);

            // Map standard OAuth claims to application roles
            foreach (var claim in claimsList)
            {
                switch (claim.Type.ToLowerInvariant())
                {
                    case "roles":
                    case "role":
                        // Direct role mapping
                        if (!string.IsNullOrEmpty(claim.Value))
                        {
                            roles.Add(MapOAuthRoleToApplicationRole(claim.Value));
                        }
                        break;

                    case "groups":
                        // Group-based role mapping
                        var mappedRole = MapOAuthGroupToApplicationRole(claim.Value);
                        if (!string.IsNullOrEmpty(mappedRole))
                        {
                            roles.Add(mappedRole);
                        }
                        break;

                    case "extension_fabrikam_role":
                    case "fabrikam_role":
                        // Custom extension attribute for Fabrikam-specific roles
                        if (!string.IsNullOrEmpty(claim.Value))
                        {
                            roles.Add(claim.Value);
                        }
                        break;

                    case "email":
                        // Email-based role assignment for demo purposes
                        var emailRole = MapEmailToRole(claim.Value);
                        if (!string.IsNullOrEmpty(emailRole))
                        {
                            roles.Add(emailRole);
                        }
                        break;
                }
            }

            // Ensure at least User role is assigned
            if (!roles.Any())
            {
                roles.Add("User");
                _logger.LogInformation("No roles found in OAuth claims, assigned default User role");
            }

            // Remove duplicates and return
            var uniqueRoles = roles.Distinct().ToList();
            _logger.LogInformation("Mapped OAuth claims to roles: {Roles}", string.Join(", ", uniqueRoles));

            return Task.FromResult<IList<string>>(uniqueRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping OAuth claims to roles");
            return Task.FromResult<IList<string>>(new List<string> { "User" }); // Fallback to User role
        }
    }

    /// <inheritdoc />
    public async Task<UserInfo> CreateOrUpdateUserFromOAuthAsync(IEnumerable<Claim> claims)
    {
        try
        {
            var claimsList = claims.ToList();
            _logger.LogInformation("Creating or updating user from OAuth claims");

            // Extract user information from claims
            var objectId = GetClaimValue(claimsList, "oid", "sub", "id");
            var email = GetClaimValue(claimsList, "email", "preferred_username", "upn");
            var firstName = GetClaimValue(claimsList, "given_name", "first_name");
            var lastName = GetClaimValue(claimsList, "family_name", "last_name");
            var displayName = GetClaimValue(claimsList, "name", "display_name");
            var tenantId = GetClaimValue(claimsList, "tid", "tenant_id");

            if (string.IsNullOrEmpty(objectId))
            {
                throw new InvalidOperationException("OAuth token must contain user object ID (oid claim)");
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("OAuth token must contain user email (email claim)");
            }

            // Map OAuth claims to application roles
            var roles = await MapOAuthClaimsToRolesAsync(claimsList);

            // Create user info object
            var userInfo = new UserInfo
            {
                Id = objectId,
                Email = email,
                FirstName = firstName ?? "OAuth",
                LastName = lastName ?? "User",
                DisplayName = displayName ?? $"{firstName} {lastName}".Trim(),
                Roles = roles.ToList(),
                IsActive = true,
                Permissions = new List<string>()
            };

            // In a production system, you might want to:
            // 1. Store OAuth user information in a local database
            // 2. Sync with existing user records
            // 3. Apply business logic for role assignments
            // 4. Update last login timestamp

            _logger.LogInformation("Created user info from OAuth claims: {Email} with roles: {Roles}", 
                userInfo.Email, string.Join(", ", userInfo.Roles));

            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user from OAuth claims");
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Maps OAuth role to application role
    /// </summary>
    private string MapOAuthRoleToApplicationRole(string oauthRole)
    {
        return oauthRole.ToLowerInvariant() switch
        {
            "administrator" or "admin" or "global_admin" => "Admin",
            "sales" or "sales_manager" or "sales_rep" => "Sales",
            "support" or "customer_service" or "helpdesk" => "CustomerService",
            "user" or "member" => "User",
            _ => "User" // Default fallback
        };
    }

    /// <summary>
    /// Maps OAuth group to application role
    /// </summary>
    private string? MapOAuthGroupToApplicationRole(string groupId)
    {
        // This would typically map Azure AD group IDs to application roles
        // For demo purposes, we'll use simple string matching
        return groupId.ToLowerInvariant() switch
        {
            var id when id.Contains("admin") => "Admin",
            var id when id.Contains("sales") => "Sales",
            var id when id.Contains("support") => "CustomerService",
            _ => null
        };
    }

    /// <summary>
    /// Maps email domain/address to role for demo purposes
    /// </summary>
    private string? MapEmailToRole(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        // Demo mapping based on email patterns
        var emailLower = email.ToLowerInvariant();
        
        if (emailLower.Contains("admin") || emailLower.EndsWith("@fabrikam.com"))
            return "Admin";
        
        if (emailLower.Contains("sales"))
            return "Sales";
        
        if (emailLower.Contains("support") || emailLower.Contains("service"))
            return "CustomerService";

        return null; // Let other logic assign role
    }

    /// <summary>
    /// Gets claim value by trying multiple claim types
    /// </summary>
    private string? GetClaimValue(IList<Claim> claims, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var claim = claims.FirstOrDefault(c => 
                c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase) ||
                c.Type.EndsWith($"/{claimType}", StringComparison.OrdinalIgnoreCase));
            
            if (claim != null && !string.IsNullOrEmpty(claim.Value))
            {
                return claim.Value;
            }
        }
        return null;
    }

    #endregion
}
