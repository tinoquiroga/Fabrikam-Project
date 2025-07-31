using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FabrikamContracts.DTOs;
using FabrikamMcp.Models; // For FabrikamDbContext
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FabrikamMcp.Services;

/// <summary>
/// Service for managing user registration and GUID validation across all authentication modes
/// </summary>
public interface IUserRegistrationService
{
    /// <summary>
    /// Register a new user for Disabled authentication mode
    /// </summary>
    Task<Guid> RegisterDisabledModeUserAsync(string name, string email, string? organization = null, string? sessionId = null);

    /// <summary>
    /// Validate that a Microsoft GUID exists in the user registry
    /// </summary>
    Task<bool> ValidateUserGuidAsync(Guid userGuid);

    /// <summary>
    /// Get or create audit GUID for authenticated JWT user
    /// </summary>
    Task<Guid> GetOrCreateAuditGuidAsync(string jwtToken);

    /// <summary>
    /// Get or create audit GUID for OAuth user
    /// </summary>
    Task<Guid> GetOrCreateAuditGuidForOAuthUserAsync(string objectId, Dictionary<string, string> oAuthClaims);

    /// <summary>
    /// Get user details by GUID for audit correlation
    /// </summary>
    Task<BaseUserRegistration?> GetUserByGuidAsync(Guid userGuid);
}

/// <summary>
/// Implementation of user registration service with database integration
/// </summary>
public class UserRegistrationService : IUserRegistrationService
{
    private readonly FabrikamDbContext _context;
    private readonly ILogger<UserRegistrationService> _logger;
    private readonly IMemoryCache _cache;
    private readonly FabrikamContracts.DTOs.GuidValidationSettings _guidSettings;

    public UserRegistrationService(
        FabrikamDbContext context,
        ILogger<UserRegistrationService> logger,
        IMemoryCache cache,
        IOptions<FabrikamContracts.DTOs.AuthenticationSettings> authSettings)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
        _guidSettings = authSettings.Value.GuidValidation;
    }

    /// <summary>
    /// Register a new user for Disabled authentication mode
    /// </summary>
    public async Task<Guid> RegisterDisabledModeUserAsync(string name, string email, string? organization = null, string? sessionId = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            throw new ArgumentException("Valid email is required", nameof(email));

        // Check if user already exists by email
        var existingUser = await _context.DisabledModeUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        
        if (existingUser != null)
        {
            _logger.LogInformation("Returning existing GUID {UserGuid} for email {Email}", 
                existingUser.Id, email);
            return existingUser.Id;
        }

        // Generate new Microsoft GUID
        var userGuid = Guid.NewGuid();
        
        var user = new DisabledModeUser
        {
            Id = userGuid,
            AuditGuid = userGuid, // For disabled mode, GUID and audit GUID are the same
            Name = name.Trim(),
            Email = email.ToLower().Trim(),
            Organization = organization?.Trim(),
            SessionId = sessionId?.Trim(),
            RegistrationDate = DateTime.UtcNow,
            AuthenticationMode = FabrikamContracts.DTOs.AuthenticationMode.Disabled
        };
        
        await _context.DisabledModeUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Registered new disabled mode user {UserGuid} for {Name} ({Email})", 
            userGuid, name, email);
        
        return userGuid;
    }

    /// <summary>
    /// Validate that a Microsoft GUID exists in the user registry
    /// </summary>
    public async Task<bool> ValidateUserGuidAsync(Guid userGuid)
    {
        if (userGuid == Guid.Empty)
            return false;

        // Check cache first if enabled
        if (_guidSettings.ValidationCacheMinutes > 0)
        {
            var cacheKey = $"guid_validation_{userGuid}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            // Check database
            var exists = await _context.DisabledModeUsers
                .AnyAsync(u => u.Id == userGuid);

            // Cache result
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_guidSettings.ValidationCacheMinutes)
            };
            _cache.Set(cacheKey, exists, cacheOptions);

            return exists;
        }

        // Direct database check if caching is disabled
        return await _context.DisabledModeUsers.AnyAsync(u => u.Id == userGuid);
    }

    /// <summary>
    /// Get or create audit GUID for authenticated JWT user
    /// </summary>
    public async Task<Guid> GetOrCreateAuditGuidAsync(string jwtToken)
    {
        var principal = ValidateJwtToken(jwtToken);
        var userId = principal.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("User ID not found in JWT");
        
        // Check if user already has an audit GUID
        var existingUser = await _context.AuthenticatedUsers
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        if (existingUser != null)
        {
            existingUser.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingUser.AuditGuid;
        }
        
        // Create new audit GUID for user
        var auditGuid = Guid.NewGuid();
        var user = new AuthenticatedUser
        {
            UserId = userId,
            AuditGuid = auditGuid,
            Email = principal.FindFirst("email")?.Value ?? "",
            Name = principal.FindFirst("name")?.Value ?? "",
            Roles = string.Join(",", principal.FindAll("role").Select(c => c.Value)),
            LastLoginDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow,
            AuthenticationMode = FabrikamContracts.DTOs.AuthenticationMode.BearerToken
        };
        
        await _context.AuthenticatedUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created audit GUID {AuditGuid} for authenticated user {UserId}", 
            auditGuid, userId);
        
        return auditGuid;
    }

    /// <summary>
    /// Get or create audit GUID for OAuth user
    /// </summary>
    public async Task<Guid> GetOrCreateAuditGuidForOAuthUserAsync(string objectId, Dictionary<string, string> oAuthClaims)
    {
        if (string.IsNullOrWhiteSpace(objectId))
            throw new ArgumentException("Azure Object ID is required", nameof(objectId));

        // Check if OAuth user already has an audit GUID
        var existingUser = await _context.OAuthUsers
            .FirstOrDefaultAsync(u => u.AzureObjectId == objectId);
        
        if (existingUser != null)
        {
            existingUser.LastOAuthLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingUser.AuditGuid;
        }
        
        // Create new audit GUID for OAuth user
        var auditGuid = Guid.NewGuid();
        var user = new OAuthUser
        {
            AzureObjectId = objectId,
            AuditGuid = auditGuid,
            Email = oAuthClaims.GetValueOrDefault("email") ?? "",
            Name = oAuthClaims.GetValueOrDefault("name") ?? "",
            TenantId = oAuthClaims.GetValueOrDefault("tid"),
            GrantedScopes = string.Join(" ", oAuthClaims.GetValueOrDefault("scp", "").Split(' ')),
            LastOAuthLoginDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow,
            AuthenticationMode = FabrikamContracts.DTOs.AuthenticationMode.EntraExternalId
        };
        
        await _context.OAuthUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created audit GUID {AuditGuid} for OAuth user {ObjectId}", 
            auditGuid, objectId);
        
        return auditGuid;
    }

    /// <summary>
    /// Get user details by GUID for audit correlation
    /// </summary>
    public async Task<BaseUserRegistration?> GetUserByGuidAsync(Guid userGuid)
    {
        if (userGuid == Guid.Empty)
            return null;

        // Try to find in disabled mode users first
        var disabledUser = await _context.DisabledModeUsers
            .FirstOrDefaultAsync(u => u.Id == userGuid);
        if (disabledUser != null)
            return disabledUser;

        // Try authenticated users
        var authUser = await _context.AuthenticatedUsers
            .FirstOrDefaultAsync(u => u.AuditGuid == userGuid);
        if (authUser != null)
            return authUser;

        // Try OAuth users
        var oauthUser = await _context.OAuthUsers
            .FirstOrDefaultAsync(u => u.AuditGuid == userGuid);
        return oauthUser;
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate and parse JWT token (simplified version)
    /// </summary>
    private ClaimsPrincipal ValidateJwtToken(string jwtToken)
    {
        // This should use proper JWT validation with your JWT settings
        // For now, this is a placeholder - implement with proper JWT validation
        var handler = new JwtSecurityTokenHandler();
        // Add proper validation logic here
        throw new NotImplementedException("JWT validation implementation needed");
    }
}
