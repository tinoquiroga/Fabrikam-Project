using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;
using FabrikamContracts.DTOs;
using Microsoft.EntityFrameworkCore;
using FabrikamApi.Data;

namespace FabrikamTests.Helpers;

/// <summary>
/// Base class for API integration tests that need to test authentication scenarios
/// Supports both authenticated and unauthenticated testing patterns
/// </summary>
public abstract class AuthenticationTestBase : IClassFixture<AuthenticationTestApplicationFactory>, IDisposable
{
    protected readonly AuthenticationTestApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly HttpClient AuthenticatedClient;
    protected readonly string ValidJwtToken;

    protected AuthenticationTestBase(AuthenticationTestApplicationFactory factory)
    {
        Factory = factory;
        
        // Create standard client (no authentication)
        Client = Factory.CreateClient();
        
        // Generate test JWT token
        ValidJwtToken = JwtTokenHelper.GenerateTestToken();
        
        // Create authenticated client
        AuthenticatedClient = CreateAuthenticatedClient(ValidJwtToken);
    }

    /// <summary>
    /// Creates an HttpClient with authentication headers using the provided JWT token
    /// </summary>
    /// <param name="jwtToken">JWT token to use for authentication</param>
    /// <returns>HttpClient configured with Bearer token authentication</returns>
    protected HttpClient CreateAuthenticatedClient(string jwtToken)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        return client;
    }

    /// <summary>
    /// Creates an HttpClient with admin-level authentication
    /// </summary>
    /// <returns>HttpClient configured with admin Bearer token</returns>
    protected HttpClient CreateAdminClient()
    {
        var adminToken = JwtTokenHelper.GenerateAdminToken();
        return CreateAuthenticatedClient(adminToken);
    }

    /// <summary>
    /// Creates an HttpClient with manager-level authentication
    /// </summary>
    /// <returns>HttpClient configured with manager Bearer token</returns>
    protected HttpClient CreateManagerClient()
    {
        var managerToken = JwtTokenHelper.GenerateManagerToken();
        return CreateAuthenticatedClient(managerToken);
    }

    /// <summary>
    /// Creates an HttpClient with an expired token for testing authentication failures
    /// </summary>
    /// <returns>HttpClient configured with expired Bearer token</returns>
    protected HttpClient CreateExpiredTokenClient()
    {
        var expiredToken = JwtTokenHelper.GenerateExpiredToken();
        return CreateAuthenticatedClient(expiredToken);
    }

    /// <summary>
    /// Creates an HttpClient with an invalid token for testing authentication failures
    /// </summary>
    /// <returns>HttpClient configured with invalid Bearer token</returns>
    protected HttpClient CreateInvalidTokenClient()
    {
        var invalidToken = JwtTokenHelper.GenerateInvalidToken();
        return CreateAuthenticatedClient(invalidToken);
    }

    /// <summary>
    /// Gets the user ID from the current valid JWT token
    /// </summary>
    /// <returns>User ID string</returns>
    protected string GetCurrentUserId()
    {
        return "test-user-123"; // Default test user ID
    }

    /// <summary>
    /// Gets the username from the current valid JWT token
    /// </summary>
    /// <returns>Username string</returns>
    protected string GetCurrentUsername()
    {
        return "testuser@fabrikam.com"; // Default test username
    }

    public virtual void Dispose()
    {
        Client?.Dispose();
        AuthenticatedClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Test application factory that can be configured for different authentication scenarios
/// This factory supports both forcing authentication mode and using environment defaults
/// </summary>
public class AuthenticationTestApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Whether to force authentication mode regardless of environment
    /// </summary>
    public bool ForceAuthenticationMode { get; set; } = false;

    /// <summary>
    /// Authentication mode to force (if ForceAuthenticationMode is true)
    /// </summary>
    public AuthenticationMode ForcedAuthenticationMode { get; set; } = AuthenticationMode.BearerToken;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Set testing environment
            context.HostingEnvironment.EnvironmentName = "Test";
            
            var configValues = new Dictionary<string, string?>
            {
                {"Database:Provider", "InMemory"},
                {"SeedData:EnableSeedOnStartup", "true"},
                {"SeedData:Method", "Json"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"},
                
                // JWT configuration matching the API's expected configuration structure
                // API uses Authentication:AspNetIdentity:Jwt section for JWT settings
                {"Authentication:AspNetIdentity:Jwt:Issuer", "FabrikamTestIssuer"},
                {"Authentication:AspNetIdentity:Jwt:Audience", "FabrikamTestAudience"},
                {"Authentication:AspNetIdentity:Jwt:SecretKey", "TestSecretKeyForFabrikamAuthenticationTestsOnly_MustBeAtLeast32Characters!"},
                {"Authentication:AspNetIdentity:Jwt:ValidateIssuer", "true"},
                {"Authentication:AspNetIdentity:Jwt:ValidateAudience", "true"},
                {"Authentication:AspNetIdentity:Jwt:ValidateLifetime", "true"},
                {"Authentication:AspNetIdentity:Jwt:ValidateIssuerSigningKey", "true"},
                {"Authentication:AspNetIdentity:Jwt:ClockSkewInMinutes", "5"},
                {"Authentication:AspNetIdentity:Jwt:ExpirationInMinutes", "60"},
                
                // Also set contracts-compatible config for authentication settings (used by auth configuration layer)
                {"Authentication:Jwt:Issuer", "FabrikamTestIssuer"},
                {"Authentication:Jwt:Audience", "FabrikamTestAudience"},
                {"Authentication:Jwt:SecretKey", "TestSecretKeyForFabrikamAuthenticationTestsOnly_MustBeAtLeast32Characters!"},
                {"Authentication:Jwt:ValidateIssuer", "true"},
                {"Authentication:Jwt:ValidateAudience", "true"},
                {"Authentication:Jwt:ValidateLifetime", "true"},
                {"Authentication:Jwt:ValidateIssuerSigningKey", "true"},
                {"Authentication:Jwt:ClockSkewInMinutes", "5"},
                {"Authentication:Jwt:ExpirationMinutes", "60"},
                {"Authentication:Jwt:RequireHttpsMetadata", "false"}
            };

            // For Phase 3 tests: Set authentication mode appropriately
            if (!ForceAuthenticationMode)
            {
                // Default to BearerToken for normal testing (since API always requires auth per user clarification)
                configValues["Authentication:Mode"] = "BearerToken";
            }
            else
            {
                // Force specific authentication mode for test scenarios
                configValues["Authentication:Mode"] = ForcedAuthenticationMode.ToString();
            }
            
            // API always requires user authentication per user clarification
            configValues["Authentication:RequireUserAuthentication"] = "true";
            configValues["Authentication:RequireServiceAuthentication"] = "true";

            config.AddInMemoryCollection(configValues);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration for replacement with in-memory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FabrikamIdentityDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with unique database per test class
            var databaseName = $"TestDatabase_{GetType().Name}_{Environment.CurrentManagedThreadId}";
            services.AddDbContext<FabrikamIdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging(); // Useful for debugging tests
            });

            // Configure JWT authentication for testing when using BearerToken mode
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = JwtTokenHelper.GetTestTokenValidationParameters();
                
                // Disable HTTPS requirement for testing
                options.RequireHttpsMetadata = false;
                
                // Configure event handlers for better test debugging
                options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine($"JWT Token validated for user: {context.Principal?.Identity?.Name}");
                            return Task.CompletedTask;
                        }
                    };
                });
        });
    }
}

/// <summary>
/// Specialized factory for testing scenarios where authentication must be enabled
/// </summary>
public class ForceAuthenticatedTestApplicationFactory : AuthenticationTestApplicationFactory
{
    public ForceAuthenticatedTestApplicationFactory()
    {
        ForceAuthenticationMode = true;
        ForcedAuthenticationMode = AuthenticationMode.BearerToken;
    }
}

/// <summary>
/// Specialized factory for testing scenarios where authentication must be disabled
/// </summary>
public class ForceUnauthenticatedTestApplicationFactory : AuthenticationTestApplicationFactory
{
    public ForceUnauthenticatedTestApplicationFactory()
    {
        ForceAuthenticationMode = true;
        ForcedAuthenticationMode = AuthenticationMode.Disabled;
    }
}
