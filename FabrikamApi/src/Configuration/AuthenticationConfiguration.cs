using Microsoft.AspNetCore.Authorization;
using FabrikamContracts.DTOs;
using FabrikamApi.Services.Authentication;

namespace FabrikamApi.Configuration;

/// <summary>
/// Environment-aware authentication configuration
/// Provides different authorization behavior based on environment and authentication mode
/// </summary>
public static class AuthenticationConfiguration
{
    /// <summary>
    /// Configure authentication and authorization based on current environment and settings
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">Web host environment</param>
    public static void ConfigureEnvironmentAwareAuthentication(this IServiceCollection services, 
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Get authentication settings with environment-aware defaults
        var authSettings = configuration.GetSection("Authentication").Get<AuthenticationSettings>() 
                          ?? new AuthenticationSettings();
        
        // Register authentication settings for dependency injection
        services.AddSingleton(authSettings);
        
        // Conditionally register JWT service based on authentication mode
        if (authSettings.Mode != AuthenticationMode.Disabled)
        {
            services.AddScoped<IJwtService, JwtService>();
        }
        else
        {
            // In disabled mode, register a no-op JWT service to satisfy dependencies
            services.AddScoped<IJwtService, DisabledJwtService>();
        }
        
        // Configure authorization policies based on authentication mode
        services.AddAuthorization(options =>
        {
            ConfigureApiAccessPolicy(options, authSettings);
            ConfigureRoleBasedPolicies(options, authSettings);
            ConfigureServiceAccessPolicy(options, authSettings);
        });
    }

    /// <summary>
    /// Configure the main API access policy - always requires authentication
    /// </summary>
    private static void ConfigureApiAccessPolicy(AuthorizationOptions options, AuthenticationSettings authSettings)
    {
        // API always requires authentication - the authSettings.Mode only affects MCP endpoint behavior
        options.AddPolicy("ApiAccess", policy =>
            policy.RequireAuthenticatedUser());
    }

    /// <summary>
    /// Configure role-based authorization policies - always require authentication
    /// </summary>
    private static void ConfigureRoleBasedPolicies(AuthorizationOptions options, AuthenticationSettings authSettings)
    {
        // API always requires authentication - role policies always require authenticated users
        
        // Read-Only access policy
        options.AddPolicy("ReadOnly", policy =>
            policy.RequireRole("Read-Only", "Read-Write", "Admin"));

        // Read-Write access policy  
        options.AddPolicy("ReadWrite", policy =>
            policy.RequireRole("Read-Write", "Admin"));

        // Admin access policy
        options.AddPolicy("Admin", policy =>
            policy.RequireRole("Admin"));
    }

    /// <summary>
    /// Configure service-to-service authentication policy (always required)
    /// </summary>
    private static void ConfigureServiceAccessPolicy(AuthorizationOptions options, AuthenticationSettings authSettings)
    {
        // Service authentication is always required regardless of mode
        options.AddPolicy("ServiceAccess", policy =>
            policy.RequireAuthenticatedUser());
    }

    /// <summary>
    /// Get the effective authentication mode for the current environment
    /// </summary>
    public static AuthenticationMode GetEffectiveAuthenticationMode(AuthenticationSettings authSettings)
    {
        return authSettings.Mode;
    }

    /// <summary>
    /// Check if user authentication is required in the current configuration
    /// </summary>
    public static bool IsUserAuthenticationRequired(AuthenticationSettings authSettings)
    {
        return authSettings.Mode != AuthenticationMode.Disabled;
    }

    /// <summary>
    /// Check if service authentication is required (always true)
    /// </summary>
    public static bool IsServiceAuthenticationRequired(AuthenticationSettings authSettings)
    {
        return authSettings.RequireServiceAuthentication;
    }
}
