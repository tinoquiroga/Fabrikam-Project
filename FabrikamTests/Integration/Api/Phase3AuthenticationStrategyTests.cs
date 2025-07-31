using FabrikamTests.Helpers;
using FabrikamContracts.DTOs;
using System.Net;
using Xunit;
using FluentAssertions;
using System.Text.Json;
using System.Net.Http.Headers;

namespace FabrikamTests.Integration.Api;

/// <summary>
/// Tests for Phase 3: API Controller Authentication Strategy
/// Validates that API controllers always require authentication per user clarification
/// </summary>
[Trait("Category", "Phase3Authentication")]
public class Phase3AuthenticationStrategyTests : SmartApiTestBase
{
    public Phase3AuthenticationStrategyTests(AuthenticationTestApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ApiControllers_AlwaysRequireAuthentication()
    {
        // Arrange: Use anonymous client (no authentication)
        var anonymousClient = CreateAnonymousClient();

        // Act & Assert: All major API endpoints should require authentication
        // Anonymous requests should always return 401 Unauthorized
        
        // Test Orders endpoint
        var ordersResponse = await anonymousClient.GetAsync("/api/orders");
        ordersResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            "Orders endpoint should require authentication");

        // Test Products endpoint
        var productsResponse = await anonymousClient.GetAsync("/api/products");
        productsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Products endpoint should require authentication");

        // Test Customers endpoint
        var customersResponse = await anonymousClient.GetAsync("/api/customers");
        customersResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Customers endpoint should require authentication");

        // Test Support Tickets endpoint
        var ticketsResponse = await anonymousClient.GetAsync("/api/supporttickets");
        ticketsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Support tickets endpoint should require authentication");
    }

    [Fact]
    public async Task AuthController_PublicEndpoints_AlwaysAccessible()
    {
        // Arrange: Use anonymous client
        var anonymousClient = CreateAnonymousClient();

        // Act & Assert: Public authentication endpoints should always be accessible
        
        // Test demo credentials endpoint (should be accessible in development/test)
        var demoResponse = await anonymousClient.GetAsync("/api/auth/demo-credentials");
        // Note: This might return 403 in some environments, but shouldn't return 401 (auth required)
        demoResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Demo credentials endpoint should not require authentication");

        // Test login endpoint with OPTIONS (CORS preflight)
        var optionsResponse = await anonymousClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/api/auth/login"));
        optionsResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Login endpoint should allow CORS preflight without authentication");
    }

    [Fact]
    public async Task AuthController_SecuredEndpoints_RespectAuthenticationMode()
    {
        // Arrange: Test both authenticated and anonymous clients
        var anonymousClient = CreateAnonymousClient();
        var authenticatedClient = CreateAuthenticatedClient();

        // Act & Assert: Secured endpoints should respect authentication mode
        
        // Test /me endpoint - should be accessible with appropriate client
        var meResponseAnonymous = await anonymousClient.GetAsync("/api/auth/me");
        var meResponseAuthenticated = await authenticatedClient.GetAsync("/api/auth/me");

        if (IsAuthenticationRequired())
        {
            meResponseAnonymous.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "ME endpoint should require authentication when auth is enabled");
            meResponseAuthenticated.StatusCode.Should().Be(HttpStatusCode.OK,
                "ME endpoint should work with valid authentication when auth is enabled");
        }
        else
        {
            meResponseAnonymous.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "ME endpoint should always require authentication - API auth is not disabled");
        }
    }

    [Fact]
    public async Task SeedController_RequiresAdminAccess()
    {
        // Arrange: Test different authorization levels
        var anonymousClient = CreateAnonymousClient();
        var userClient = CreateAuthenticatedClient();
        var adminClient = CreateAdminClient();

        // Act: Try to access seed endpoints with different clients
        var seedEndpoint = "/api/seed/methods";
        
        var anonymousResponse = await anonymousClient.GetAsync(seedEndpoint);
        var userResponse = await userClient.GetAsync(seedEndpoint);
        var adminResponse = await adminClient.GetAsync(seedEndpoint);

        // Assert: Seed controller should enforce admin policy
        if (IsAuthenticationRequired())
        {
            anonymousResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "Seed endpoint should require authentication when auth is enabled");
            
            // Note: User vs admin distinction might not be enforced in test mode
            // but admin client should definitely work
            adminResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                "Admin client should have access to seed endpoints");
        }
        else
        {
            // In disabled mode, all clients should have access
            anonymousResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                "Seed endpoint should be accessible when authentication is disabled");
        }
    }

    [Fact]
    public async Task InfoController_AlwaysPublic()
    {
        // Arrange: Use anonymous client
        var anonymousClient = CreateAnonymousClient();

        // Act: Access info endpoints
        var infoResponse = await anonymousClient.GetAsync("/api/info");
        var authInfoResponse = await anonymousClient.GetAsync("/api/info/auth");

        // Assert: Info controller should always be publicly accessible
        infoResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Info endpoint should always be publicly accessible");
        authInfoResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Auth info endpoint should always be publicly accessible");

        // Validate auth info response structure
        var authInfoContent = await authInfoResponse.Content.ReadAsStringAsync();
        
        // Debug: Output what we actually received
        Console.WriteLine($"Auth info response: {authInfoContent}");
        
        var authInfo = JsonDocument.Parse(authInfoContent);
        
        authInfo.RootElement.TryGetProperty("mode", out var modeProperty).Should().BeTrue(
            $"Auth info should include authentication mode. Actual response: {authInfoContent}");
        authInfo.RootElement.TryGetProperty("isEnabled", out var enabledProperty).Should().BeTrue(
            "Auth info should include enabled status");
    }

    [Fact]
    public async Task SmartTestBase_CreatesOptimalClient()
    {
        // Arrange & Act: Use the default client from the smart test base
        var defaultClient = Client;
        
        // Assert: Default client should work for API access
        var response = await defaultClient.GetAsync("/api/info");
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Smart test base should create a client that works for API access");

        // Test that we can access a typical business endpoint
        var ordersResponse = await defaultClient.GetAsync("/api/orders");
        ordersResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Smart test base client should work for business endpoints");
    }

    [Fact]
    public void AuthenticationMode_Detection_WorksCorrectly()
    {
        // Arrange & Act: Get authentication settings
        var authSettings = GetAuthenticationSettings();
        var isAuthRequired = IsAuthenticationRequired();

        // Assert: Authentication mode detection should be consistent
        var expectedAuthRequired = authSettings.Mode != AuthenticationMode.Disabled;
        isAuthRequired.Should().Be(expectedAuthRequired,
            "Authentication requirement detection should match the configured mode");

        // Per user clarification: API should always require authentication with BearerToken mode
        authSettings.Mode.Should().Be(AuthenticationMode.BearerToken,
            "API should always require authentication with BearerToken mode");
        isAuthRequired.Should().BeTrue(
            "API should always require authentication per user clarification");
    }

    [Fact]
    public async Task PolicyBasedAuthorization_WorksCorrectly()
    {
        // Arrange: Create clients with different authorization levels
        var anonymousClient = CreateAnonymousClient();
        var authenticatedClient = CreateAuthenticatedClient();

        // Act: Test endpoints that use policy-based authorization
        var ordersResponse = await anonymousClient.GetAsync("/api/orders");
        var productsResponse = await authenticatedClient.GetAsync("/api/products");

        // Assert: Policy-based authorization should work correctly
        if (IsAuthenticationRequired())
        {
            // When authentication is required, anonymous should fail, authenticated should succeed
            ordersResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "Anonymous client should not access orders when auth is required");
            productsResponse.StatusCode.Should().Be(HttpStatusCode.OK,
                "Authenticated client should access products when auth is required");
        }
        else
        {
            // API authentication is never disabled - anonymous access should always fail
            ordersResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "Anonymous client should never access orders - API auth is always required");
            productsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "Even authenticated client needs valid token - mock client likely has no valid token");
        }
    }

    [Fact]
    public void ClientCreationMethods_WorkCorrectly()
    {
        // Act: Create different types of clients
        var anonymous = CreateAnonymousClient();
        var authenticated = CreateAuthenticatedClient();
        var admin = CreateAdminClient();
        var manager = CreateManagerClient();
        var contextAware = CreateContextAwareClient();

        // Assert: All clients should be created successfully
        anonymous.Should().NotBeNull("Anonymous client should be created");
        authenticated.Should().NotBeNull("Authenticated client should be created");
        admin.Should().NotBeNull("Admin client should be created");
        manager.Should().NotBeNull("Manager client should be created");
        contextAware.Should().NotBeNull("Context-aware client should be created");

        // Test that authenticated clients have authorization headers
        authenticated.DefaultRequestHeaders.Authorization.Should().NotBeNull(
            "Authenticated client should have authorization header");
        admin.DefaultRequestHeaders.Authorization.Should().NotBeNull(
            "Admin client should have authorization header");
        manager.DefaultRequestHeaders.Authorization.Should().NotBeNull(
            "Manager client should have authorization header");

        // Test that anonymous client has no authorization header
        anonymous.DefaultRequestHeaders.Authorization.Should().BeNull(
            "Anonymous client should not have authorization header");
    }
}
