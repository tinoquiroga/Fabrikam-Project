using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using FluentAssertions;
using FabrikamApi.DTOs;
using Xunit;

namespace FabrikamTests.Api;

[Trait("Category", "Api")]
public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task GetOrders_ReturnsExpectedStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        response.Headers.Contains("X-Total-Count").Should().BeTrue();
        
        // Validate order structure if orders exist
        if (result.RootElement.GetArrayLength() > 0)
        {
            var firstOrder = result.RootElement[0];
            firstOrder.TryGetProperty("id", out _).Should().BeTrue();
            firstOrder.TryGetProperty("orderNumber", out _).Should().BeTrue();
            firstOrder.TryGetProperty("status", out _).Should().BeTrue();
            firstOrder.TryGetProperty("customer", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetSalesAnalytics_ReturnsCorrectStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/analytics");
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var analytics = JsonSerializer.Deserialize<SalesAnalyticsDto>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        // Validate structure matches what MCP tools expect
        analytics.Should().NotBeNull();
        analytics!.Summary.Should().NotBeNull();
        analytics.ByStatus.Should().NotBeNull();
        analytics.ByRegion.Should().NotBeNull();
        analytics.DailyTrends.Should().NotBeNull();
        
        analytics.Summary.TotalOrders.Should().BeGreaterOrEqualTo(0);
        analytics.Summary.TotalRevenue.Should().BeGreaterOrEqualTo(0);
        analytics.Summary.AverageOrderValue.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetSalesAnalytics_WithDateRange_ReturnsFilteredData()
    {
        // Arrange
        var fromDate = "2025-01-01";
        var toDate = "2025-12-31";

        // Act
        var response = await _client.GetAsync($"/api/orders/analytics?fromDate={fromDate}&toDate={toDate}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var analytics = JsonSerializer.Deserialize<SalesAnalyticsDto>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        analytics.Should().NotBeNull();
        analytics!.Summary.Period.Should().NotBeNull();
    }

    [Theory]
    [InlineData("/api/orders")]
    [InlineData("/api/orders/analytics")]
    [InlineData("/api/customers")]
    [InlineData("/api/products")]
    [InlineData("/api/supporttickets")]
    [InlineData("/api/info")]
    public async Task AllEndpoints_ReturnSuccess(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetOrder_WithValidId_ReturnsOrder()
    {
        // First, get orders to find a valid ID
        var ordersResponse = await _client.GetAsync("/api/orders");
        var ordersContent = await ordersResponse.Content.ReadAsStringAsync();
        var ordersResult = JsonDocument.Parse(ordersContent);
        
        if (ordersResult.RootElement.ValueKind == JsonValueKind.Array && 
            ordersResult.RootElement.GetArrayLength() > 0)
        {
            var firstOrder = ordersResult.RootElement[0];
            if (firstOrder.TryGetProperty("id", out var idElement))
            {
                var orderId = idElement.GetInt32();

                // Act
                var response = await _client.GetAsync($"/api/orders/{orderId}");

                // Assert
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var order = JsonDocument.Parse(content);
                order.RootElement.TryGetProperty("id", out _).Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task GetOrder_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/999999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
