using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using FluentAssertions;
using FabrikamContracts.DTOs.Customers;
using FabrikamTests.Helpers;
using Xunit;

namespace FabrikamTests.Integration.Api;

[Trait("Category", "Api")]
public class CustomersControllerTests : AuthenticationTestBase
{
    public CustomersControllerTests(AuthenticationTestApplicationFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task GetCustomers_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task GetCustomers_ReturnsExpectedStructure()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        // Deserialize to DTOs for type-safe validation
        var customers = JsonSerializer.Deserialize<CustomerListItemDto[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customers.Should().NotBeNull();
        customers!.Length.Should().BeGreaterThan(0, "should return customer data");
        
        // Validate DTO structure with first customer
        var firstCustomer = customers.First();
        firstCustomer.Id.Should().BeGreaterThan(0);
        firstCustomer.Name.Should().NotBeNullOrEmpty();
        firstCustomer.Email.Should().NotBeNullOrEmpty();
        firstCustomer.Region.Should().NotBeNullOrEmpty();
        firstCustomer.OrderCount.Should().BeGreaterThanOrEqualTo(0);
        firstCustomer.TotalSpent.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ReturnsCustomer()
    {
        // Arrange - First get all customers to find a valid ID
        var allCustomersResponse = await AuthenticatedClient.GetAsync("/api/customers");
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
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);
        
        result.RootElement.GetProperty("id").GetInt32().Should().Be(customerId);
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers/99999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("Pacific Northwest")]
    [InlineData("Midwest")]
    [InlineData("Northeast")]
    public async Task GetCustomers_WithRegionFilter_ReturnsFilteredResults(string region)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/customers?region={Uri.EscapeDataString(region)}");

        // Assert
        response.EnsureSuccessStatusCode();
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
    public async Task GetCustomersAnalytics_ReturnsSuccessAndCorrectStructure()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers/analytics");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        
        // Check for expected analytics properties
        result.RootElement.TryGetProperty("totalCustomers", out _).Should().BeTrue();
        result.RootElement.TryGetProperty("activeCustomers", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("Pacific Northwest")]
    [InlineData("Midwest")]
    [InlineData("Northeast")]
    public async Task GetCustomersAnalytics_WithRegionFilter_ReturnsRegionalData(string region)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/customers/analytics?region={Uri.EscapeDataString(region)}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        
        // Should have analytics data for the specific region
        result.RootElement.TryGetProperty("totalCustomers", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomer_WithOrders_IncludesOrderData()
    {
        // Arrange - Find a customer with orders
        var customersResponse = await AuthenticatedClient.GetAsync("/api/customers?includeOrders=true");
        var customersContent = await customersResponse.Content.ReadAsStringAsync();
        var customers = JsonDocument.Parse(customersContent);
        
        JsonElement? customerWithOrders = null;
        foreach (var customer in customers.RootElement.EnumerateArray())
        {
            if (customer.TryGetProperty("orders", out var ordersProperty) && 
                ordersProperty.ValueKind == JsonValueKind.Array && 
                ordersProperty.GetArrayLength() > 0)
            {
                customerWithOrders = customer;
                break;
            }
        }

        if (customerWithOrders == null)
        {
            return; // Skip test if no customer with orders found
        }

        var customerId = customerWithOrders.Value.GetProperty("id").GetInt32();

        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/customers/{customerId}?includeOrders=true");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.TryGetProperty("orders", out var includedOrders).Should().BeTrue();
        includedOrders.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 3)]
    [InlineData(1, 10)]
    public async Task GetCustomers_WithPagination_ReturnsCorrectPageSize(int page, int pageSize)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/customers?page={page}&pageSize={pageSize}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Check that we don't exceed the requested page size
        result.RootElement.GetArrayLength().Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task GetCustomers_WithInvalidRegion_ReturnsEmptyArray()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers?region=InvalidRegion");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        result.RootElement.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetCustomersAnalytics_WithInvalidRegion_ReturnsZeroMetrics()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers/analytics?region=InvalidRegion");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        
        if (result.RootElement.TryGetProperty("totalCustomers", out var totalProperty))
        {
            totalProperty.GetInt32().Should().Be(0);
        }
    }

    [Fact]
    public async Task GetCustomers_ResponseTimeIsReasonable()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers");

        // Assert
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should respond within 1 second
    }

    [Fact]
    public async Task GetCustomersAnalytics_ResponseTimeIsReasonable()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers/analytics");

        // Assert
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Analytics might take slightly longer
    }
}
