using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FabrikamApi;
using FabrikamContracts.DTOs;

namespace FabrikamTests.Integration.Authentication;

public class EntraExternalIdAuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EntraExternalIdAuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:Mode"] = "EntraExternalId",
                    ["Authentication:EntraExternalId:TenantId"] = "test-tenant.onmicrosoft.com",
                    ["Authentication:EntraExternalId:ClientId"] = "test-client-id",
                    ["Authentication:EntraExternalId:ClientSecret"] = "test-client-secret",
                    ["Authentication:EntraExternalId:Authority"] = "https://test-tenant.b2clogin.com/test-tenant.onmicrosoft.com/v2.0",
                    ["Authentication:EntraExternalId:Scopes:0"] = "openid",
                    ["Authentication:EntraExternalId:Scopes:1"] = "profile",
                    ["Authentication:EntraExternalId:Scopes:2"] = "email"
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SwaggerUI_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AuthenticationMode_ShouldBeEntraExternalId()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Act
        var authMode = configuration.GetValue<string>("Authentication:Mode");

        // Assert
        Assert.Equal("EntraExternalId", authMode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutAuthentication_ShouldRedirectToOAuth()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        // In EntraExternalId mode, unauthenticated requests should be redirected to OAuth
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/orders")]
    [InlineData("/api/products")]
    [InlineData("/api/analytics")]
    [InlineData("/api/customers")]
    public async Task ProtectedEndpoints_WithoutAuth_ShouldRequireAuthentication(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                   response.StatusCode == System.Net.HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task OAuthConfiguration_ShouldBeValidlyConfigured()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Act
        var tenantId = configuration.GetValue<string>("Authentication:EntraExternalId:TenantId");
        var clientId = configuration.GetValue<string>("Authentication:EntraExternalId:ClientId");
        var authority = configuration.GetValue<string>("Authentication:EntraExternalId:Authority");

        // Assert
        Assert.Equal("test-tenant.onmicrosoft.com", tenantId);
        Assert.Equal("test-client-id", clientId);
        Assert.Equal("https://test-tenant.b2clogin.com/test-tenant.onmicrosoft.com/v2.0", authority);
    }

    [Fact]
    public async Task AuthenticationService_ShouldBeRegistered()
    {
        // Arrange
        var serviceProvider = _factory.Services;

        // Act & Assert
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();
        
        Assert.NotNull(authService);
    }

    [Fact]
    public async Task JwtBearerAuthentication_ShouldBeConfigured()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();

        // Act
        var authSchemes = scope.ServiceProvider.GetServices<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();

        // Assert
        Assert.NotEmpty(authSchemes);
    }

    // Simulated OAuth token tests (in real scenarios, you'd use actual OAuth tokens)
    [Fact]
    public async Task MockOAuthClaims_ShouldMapToCorrectRoles()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();

        var adminClaims = new List<System.Security.Claims.Claim>
        {
            new("oid", "admin-user-id"),
            new("email", "admin@fabrikam.com"),
            new("given_name", "Admin"),
            new("family_name", "User"),
            new("roles", "administrator")
        };

        // Act
        var roles = await authService.MapOAuthClaimsToRolesAsync(adminClaims);

        // Assert
        Assert.Contains("Admin", roles);
    }

    [Fact]
    public async Task MockOAuthUser_ShouldBeCreatedFromClaims()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();

        var userClaims = new List<System.Security.Claims.Claim>
        {
            new("oid", "test-user-123"),
            new("email", "testuser@example.com"),
            new("given_name", "Test"),
            new("family_name", "User"),
            new("name", "Test User"),
            new("roles", "sales")
        };

        // Act
        var userInfo = await authService.CreateOrUpdateUserFromOAuthAsync(userClaims);

        // Assert
        Assert.NotNull(userInfo);
        Assert.Equal("test-user-123", userInfo.Id);
        Assert.Equal("testuser@example.com", userInfo.Email);
        Assert.Equal("Test", userInfo.FirstName);
        Assert.Equal("User", userInfo.LastName);
        Assert.Contains("Sales", userInfo.Roles);
        Assert.True(userInfo.IsActive);
    }

    [Theory]
    [InlineData("admin@fabrikam.com", "Admin")]
    [InlineData("sales.person@example.com", "Sales")]
    [InlineData("support.agent@example.com", "CustomerService")]
    [InlineData("regular.user@example.com", "User")]
    public async Task EmailBasedRoleMapping_ShouldWorkCorrectly(string email, string expectedRole)
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();

        var claims = new List<System.Security.Claims.Claim>
        {
            new("oid", "test-user-id"),
            new("email", email),
            new("given_name", "Test"),
            new("family_name", "User")
        };

        // Act
        var userInfo = await authService.CreateOrUpdateUserFromOAuthAsync(claims);

        // Assert
        Assert.Contains(expectedRole, userInfo.Roles);
    }

    [Fact]
    public async Task MultipleRoleClaims_ShouldBeHandledCorrectly()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();

        var claims = new List<System.Security.Claims.Claim>
        {
            new("oid", "multi-role-user"),
            new("email", "manager@fabrikam.com"),
            new("roles", "sales"),
            new("roles", "admin"),
            new("groups", "support-group")
        };

        // Act
        var roles = await authService.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("Admin", roles);
        Assert.Contains("Sales", roles);
        Assert.Contains("CustomerService", roles);
        
        // Should remove duplicates
        var uniqueRoles = roles.Distinct().ToList();
        Assert.Equal(roles.Count, uniqueRoles.Count);
    }

    [Fact]
    public async Task CustomFabrikamRoleClaim_ShouldBeRespected()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();

        var claims = new List<System.Security.Claims.Claim>
        {
            new("oid", "custom-role-user"),
            new("email", "custom@fabrikam.com"),
            new("extension_fabrikam_role", "SuperAdmin"),
            new("fabrikam_role", "DataAnalyst")
        };

        // Act
        var roles = await authService.MapOAuthClaimsToRolesAsync(claims);

        // Assert
        Assert.Contains("SuperAdmin", roles);
        Assert.Contains("DataAnalyst", roles);
        Assert.Contains("Admin", roles); // From email mapping
    }

    [Fact]
    public async Task OAuthTokenValidation_WithEmptyToken_ShouldReturnNull()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();

        // Act
        var result = await authService.ValidateOAuthTokenAsync("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task OAuthTokenValidation_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();

        // Act
        var result = await authService.ValidateOAuthTokenAsync("mock-valid-token");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("oauth-validated", result.Id);
        Assert.Contains("User", result.Roles);
    }

    [Fact]
    public async Task AuthenticationConfiguration_ShouldSupportThreeModes()
    {
        // Test that our authentication configuration supports all three modes
        var testConfigurations = new[]
        {
            ("Disabled", false),
            ("BearerToken", true),
            ("EntraExternalId", true)
        };

        foreach (var (mode, shouldHaveAuth) in testConfigurations)
        {
            // Arrange
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Authentication:Mode"] = mode,
                        ["Authentication:Jwt:SecretKey"] = "test-jwt-secret-key-for-testing-minimum-256-bits",
                        ["Authentication:Jwt:Issuer"] = "test-issuer",
                        ["Authentication:Jwt:Audience"] = "test-audience",
                        ["Authentication:EntraExternalId:TenantId"] = "test-tenant",
                        ["Authentication:EntraExternalId:ClientId"] = "test-client",
                        ["Authentication:EntraExternalId:ClientSecret"] = "test-secret",
                        ["Authentication:EntraExternalId:Authority"] = "https://test.b2clogin.com/test/v2.0"
                    });
                });
            });

            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task ErrorHandling_WithInvalidOAuthClaims_ShouldHandleGracefully()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        using var scope = serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<FabrikamApi.Services.Authentication.IEntraExternalIdAuthenticationService>();

        var invalidClaims = new List<System.Security.Claims.Claim>
        {
            // Missing required oid claim
            new("email", "test@example.com")
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            authService.CreateOrUpdateUserFromOAuthAsync(invalidClaims));
    }

    [Fact]
    public async Task ApplicationStartup_WithEntraExternalIdMode_ShouldInitializeCorrectly()
    {
        // This test verifies that the application can start successfully with EntraExternalId configuration
        // The fact that the test class constructor runs without exception proves this

        // Arrange & Act - done in constructor
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }
}
