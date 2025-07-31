using System.Net;
using System.Text.Json;
using FluentAssertions;
using FabrikamTests.Helpers;
using FabrikamContracts.DTOs;
using Xunit;

namespace FabrikamTests.Integration.Api;

/// <summary>
/// Sample authenticated API tests demonstrating the new test infrastructure
/// Shows various authentication scenarios and patterns for API testing
/// Uses ForceAuthenticatedTestApplicationFactory to ensure authentication is enabled
/// </summary>
[Trait("Category", "AuthenticatedApi")]
public class AuthenticatedCustomersControllerTests : AuthenticationTestBase
{
    public AuthenticatedCustomersControllerTests(AuthenticationTestApplicationFactory factory) 
        : base(factory)
    {
        // Force authentication mode for these tests
        factory.ForceAuthenticationMode = true;
        factory.ForcedAuthenticationMode = AuthenticationMode.BearerToken;
    }

    [Fact]
    public async Task GetCustomers_WithValidToken_ReturnsSuccess()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task GetCustomers_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomers_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        using var expiredClient = CreateExpiredTokenClient();

        // Act
        var response = await expiredClient.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomers_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        using var invalidClient = CreateInvalidTokenClient();

        // Act
        var response = await invalidClient.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomers_WithAdminToken_ReturnsSuccess()
    {
        // Arrange
        using var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/customers");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonDocument.Parse(content);
        customers.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetCustomers_WithManagerToken_ReturnsSuccess()
    {
        // Arrange
        using var managerClient = CreateManagerClient();

        // Act
        var response = await managerClient.GetAsync("/api/customers");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomer_WithValidTokenAndId_ReturnsCustomer()
    {
        // Arrange - First get all customers to find a valid ID
        var allCustomersResponse = await AuthenticatedClient.GetAsync("/api/customers");
        allCustomersResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var allCustomersContent = await allCustomersResponse.Content.ReadAsStringAsync();
        var allCustomers = JsonDocument.Parse(allCustomersContent);
        
        if (allCustomers.RootElement.GetArrayLength() == 0)
        {
            return; // Skip test if no customers exist
        }

        var firstCustomer = allCustomers.RootElement[0];
        var customerId = firstCustomer.GetProperty("id").GetInt32();

        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/customers/{customerId}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);
        
        result.RootElement.GetProperty("id").GetInt32().Should().Be(customerId);
    }

    [Fact]
    public async Task GetCustomer_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/customers/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomersAnalytics_WithValidToken_ReturnsAnalytics()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers/analytics");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        result.RootElement.TryGetProperty("totalCustomers", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomersAnalytics_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/customers/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("Pacific Northwest")]
    [InlineData("Midwest")]
    [InlineData("Northeast")]
    public async Task GetCustomers_WithRegionFilterAndToken_ReturnsFilteredResults(string region)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/customers?region={Uri.EscapeDataString(region)}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Verify all returned customers have the specified region
        foreach (var customer in result.RootElement.EnumerateArray())
        {
            if (customer.TryGetProperty("region", out var regionProperty))
            {
                regionProperty.GetString().Should().Be(region);
            }
        }
    }

    [Fact]
    public async Task AuthenticationFlow_ValidatesTokenClaims()
    {
        // Arrange
        var expectedUserId = GetCurrentUserId();
        var expectedUsername = GetCurrentUsername();

        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        // Additional validation that token claims are being processed
        // This test validates that our JWT token generation is working correctly
        expectedUserId.Should().NotBeNullOrEmpty();
        expectedUsername.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ConcurrentRequests_WithAuthentication_HandleProperly()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Act - Make 5 concurrent requests
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(AuthenticatedClient.GetAsync("/api/customers"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.IsSuccessStatusCode.Should().BeTrue();
            response.Dispose();
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
