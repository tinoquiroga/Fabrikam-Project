using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;
using FabrikamContracts.DTOs;

namespace FabrikamTests.Helpers;

/// <summary>
/// Base class for API integration tests that require authentication
/// Provides authenticated HttpClient instances and JWT token management
/// </summary>
public abstract class AuthenticatedTestBase : IClassFixture<AuthenticatedTestApplicationFactory>, IDisposable
{
    protected readonly AuthenticatedTestApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly HttpClient AuthenticatedClient;
    protected readonly string ValidJwtToken;

    protected AuthenticatedTestBase(AuthenticatedTestApplicationFactory factory)
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
/// Custom WebApplicationFactory for authenticated testing
/// Configures JWT authentication and test environment settings
/// </summary>
public class AuthenticatedTestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Set testing environment to ensure proper authentication configuration
            context.HostingEnvironment.EnvironmentName = "Testing";
            
            // Add testing configuration - let environment-aware defaults handle authentication mode
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Database:Provider", "InMemory"},
                {"SeedData:EnableSeedOnStartup", "true"},
                {"SeedData:Method", "Json"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"},
                
                // Only add JWT configuration but let the mode be determined by environment
                // This allows testing both authenticated and unauthenticated scenarios
                {"Authentication:Jwt:Issuer", "FabrikamTestIssuer"},
                {"Authentication:Jwt:Audience", "FabrikamTestAudience"},
                {"Authentication:Jwt:SecretKey", "TestSecretKeyForFabrikamAuthenticationTestsOnly_MustBeAtLeast32Characters!"}
            });
        });

        builder.ConfigureServices(services =>
        {
            // Configure JWT authentication for testing
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
                        // Log authentication failures during testing (helpful for debugging)
                        Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Token validation successful
                        Console.WriteLine($"JWT Token validated for user: {context.Principal?.Identity?.Name}");
                        return Task.CompletedTask;
                    }
                };
            });
        });
    }
}
