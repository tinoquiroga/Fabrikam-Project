using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FabrikamApi.Models.Authentication;
using FabrikamApi.Services.Authentication;
using ApiAuthSettings = FabrikamApi.Models.Authentication.AuthenticationSettings;
using ApiJwtSettings = FabrikamApi.Models.Authentication.JwtSettings;
using ContractAuthMode = FabrikamContracts.DTOs.AuthenticationMode;
using ContractEntraSettings = FabrikamContracts.DTOs.EntraExternalIdSettings;

namespace FabrikamApi.Configuration;

/// <summary>
/// Configuration extensions for environment-aware authentication setup
/// Supports Disabled, BearerToken, and EntraExternalId authentication modes
/// </summary>
public static class AuthenticationConfigurationExtensions
{
    /// <summary>
    /// Configures authentication services based on environment and authentication mode
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">Web host environment</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection ConfigureEnvironmentAwareAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Get authentication mode from configuration
        var authenticationMode = GetAuthenticationMode(configuration, environment);
        
        // Log the selected authentication mode
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<Program>>();
        logger?.LogInformation("Configuring authentication mode: {AuthenticationMode}", authenticationMode);

        switch (authenticationMode)
        {
            case ContractAuthMode.Disabled:
                ConfigureDisabledAuthentication(services, configuration);
                break;

            case ContractAuthMode.BearerToken:
                ConfigureBearerTokenAuthentication(services, configuration);
                break;

            case ContractAuthMode.EntraExternalId:
                ConfigureEntraExternalIdAuthentication(services, configuration);
                break;

            default:
                throw new InvalidOperationException($"Unsupported authentication mode: {authenticationMode}");
        }

        return services;
    }

    /// <summary>
    /// Determines the authentication mode based on configuration and environment
    /// </summary>
    private static ContractAuthMode GetAuthenticationMode(IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Check explicit configuration first
        var modeString = configuration.GetValue<string>("Authentication:Mode");
        if (!string.IsNullOrEmpty(modeString) && Enum.TryParse<ContractAuthMode>(modeString, true, out var explicitMode))
        {
            return explicitMode;
        }

        // Use environment-aware defaults
        var authSettings = new FabrikamContracts.DTOs.AuthenticationSettings();
        return authSettings.Mode; // This uses the environment-aware default logic
    }

    /// <summary>
    /// Configures disabled authentication (GUID tracking only)
    /// </summary>
    private static void ConfigureDisabledAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // No authentication middleware needed for disabled mode
        // API endpoints will be open, but can still track users via GUID
        
        services.AddAuthorization(options =>
        {
            // Default policy allows all requests
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true) // Allow all requests
                .Build();
        });

        // Add a service to handle GUID tracking
        services.AddScoped<IGuidTrackingService, GuidTrackingService>();
    }

    /// <summary>
    /// Configures JWT Bearer Token authentication
    /// </summary>
    private static void ConfigureBearerTokenAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(ApiJwtSettings.SectionName).Get<ApiJwtSettings>() ?? new ApiJwtSettings();
        
        if (string.IsNullOrEmpty(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is required for BearerToken authentication mode");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewInMinutes)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT Authentication failed: {Exception}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogDebug("JWT token validated for user: {User}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                }
            };
        });

        ConfigureStandardAuthorization(services);
        
        // Register JWT-specific services
        services.AddScoped<IJwtService, JwtService>();
    }

    /// <summary>
    /// Configures Entra External ID OAuth 2.0 authentication
    /// </summary>
    private static void ConfigureEntraExternalIdAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var entraSettings = configuration.GetSection("Authentication:EntraExternalId").Get<EntraExternalIdSettings>();
        
        if (entraSettings == null)
        {
            throw new InvalidOperationException("EntraExternalId settings are required for EntraExternalId authentication mode");
        }

        // Validate settings
        entraSettings.Validate();

        // For now, we'll use JWT Bearer tokens issued by the Entra External ID
        // In a full implementation, you would add OpenID Connect middleware
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = true; // Always require HTTPS for OAuth
            options.Authority = entraSettings.Authority;
            options.Audience = entraSettings.ClientId;
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = entraSettings.Authority,
                ValidAudience = entraSettings.ClientId,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("OAuth Authentication failed: {Exception}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var authService = context.HttpContext.RequestServices.GetRequiredService<IEntraExternalIdAuthenticationService>();
                    
                    // Create or update user from OAuth claims
                    try
                    {
                        var userInfo = await authService.CreateOrUpdateUserFromOAuthAsync(context.Principal!.Claims);
                        logger.LogInformation("OAuth token validated for user: {Email}", userInfo.Email);
                        
                        // Add user info to context for later use
                        context.HttpContext.Items["UserInfo"] = userInfo;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing OAuth user claims");
                    }
                }
            };
        });

        ConfigureStandardAuthorization(services);
        
        // Register EntraExternalId-specific services
        services.AddScoped<IEntraExternalIdAuthenticationService, EntraExternalIdAuthenticationService>();
    }

    /// <summary>
    /// Configures standard authorization policies for authenticated modes
    /// </summary>
    private static void ConfigureStandardAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Default policy requires authentication
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Role-based policies
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("SalesOrAdmin", policy => policy.RequireRole("Sales", "Admin"));
            options.AddPolicy("CustomerServiceOrAdmin", policy => policy.RequireRole("CustomerService", "Admin"));
            options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
        });
    }
}

/// <summary>
/// GUID tracking service for disabled authentication mode
/// </summary>
public interface IGuidTrackingService
{
    /// <summary>
    /// Gets or creates a tracking GUID for the current request
    /// </summary>
    Task<Guid> GetOrCreateTrackingGuidAsync(HttpContext context);

    /// <summary>
    /// Validates a tracking GUID
    /// </summary>
    Task<bool> ValidateTrackingGuidAsync(Guid trackingGuid);
}

/// <summary>
/// Implementation of GUID tracking service
/// </summary>
public class GuidTrackingService : IGuidTrackingService
{
    private readonly ILogger<GuidTrackingService> _logger;

    public GuidTrackingService(ILogger<GuidTrackingService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Guid> GetOrCreateTrackingGuidAsync(HttpContext context)
    {
        // Check for existing GUID in headers or query parameters
        var guidString = context.Request.Headers["X-Tracking-Guid"].FirstOrDefault() ??
                        context.Request.Query["trackingGuid"].FirstOrDefault();

        if (Guid.TryParse(guidString, out var existingGuid))
        {
            _logger.LogDebug("Using existing tracking GUID: {TrackingGuid}", existingGuid);
            return Task.FromResult(existingGuid);
        }

        // Create new GUID
        var newGuid = Guid.NewGuid();
        _logger.LogInformation("Created new tracking GUID: {TrackingGuid}", newGuid);
        
        // Store in response headers for client reference
        context.Response.Headers.Append("X-Tracking-Guid", newGuid.ToString());
        
        return Task.FromResult(newGuid);
    }

    /// <inheritdoc />
    public Task<bool> ValidateTrackingGuidAsync(Guid trackingGuid)
    {
        // For this implementation, all GUIDs are valid
        // In a production system, you might check against a database
        var isValid = trackingGuid != Guid.Empty;
        
        _logger.LogDebug("Validated tracking GUID {TrackingGuid}: {IsValid}", trackingGuid, isValid);
        
        return Task.FromResult(isValid);
    }
}
