using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using FabrikamMcp.Tools;
using FabrikamApi.DTOs;
using Xunit;

namespace FabrikamTests.Mcp;

[Trait("Category", "Mcp")]
public class FabrikamSalesToolsTests_New
{
    [Fact]
    public async Task GetSalesAnalytics_WithValidResponse_ReturnsStructuredData()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:5235/")
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["FabrikamApi:BaseUrl"] = "https://localhost:5235"
            })
            .Build();

        var salesTools = new FabrikamSalesTools(httpClient, configuration);

        var salesAnalyticsDto = new SalesAnalyticsDto
        {
            Summary = new SalesSummaryDto
            {
                TotalOrders = 150,
                TotalRevenue = 75000.00m,
                AverageOrderValue = 500.00m
            },
            ByStatus = new List<SalesByStatusDto>
            {
                new() { Status = "Completed", Count = 100, Revenue = 50000.00m },
                new() { Status = "Pending", Count = 50, Revenue = 25000.00m }
            },
            ByRegion = new List<SalesByRegionDto>
            {
                new() { Region = "North", Count = 75, Revenue = 37500.00m },
                new() { Region = "South", Count = 75, Revenue = 37500.00m }
            },
            DailyTrends = new List<DailySalesDto>
            {
                new() { Date = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), OrderCount = 10, Revenue = 5000.00m },
                new() { Date = DateTime.Today.ToString("yyyy-MM-dd"), OrderCount = 12, Revenue = 6000.00m }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(salesAnalyticsDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await salesTools.GetSalesAnalytics();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("Summary");
        resultString.Should().Contain("TotalOrders");
        resultString.Should().Contain("150");
    }

    [Fact]
    public async Task GetOrders_WithValidResponse_ReturnsOrderData()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:5235/")
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["FabrikamApi:BaseUrl"] = "https://localhost:5235"
            })
            .Build();

        var salesTools = new FabrikamSalesTools(httpClient, configuration);

        var ordersDto = new List<OrderDetailDto>
        {
            new() { 
                Id = 1, 
                Status = "Completed", 
                Financials = new OrderFinancialDto { Total = 1000.00m },
                Customer = new OrderCustomerDto { Name = "John Doe" },
                Region = "North"
            }
        };

        var jsonResponse = JsonSerializer.Serialize(ordersDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await salesTools.GetOrders();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("John Doe");
        resultString.Should().Contain("Completed");
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Pending")]
    [InlineData("Cancelled")]
    [InlineData("Processing")]
    public async Task GetOrders_WithDifferentStatuses_FiltersCorrectly(string status)
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:5235/")
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["FabrikamApi:BaseUrl"] = "https://localhost:5235"
            })
            .Build();

        var salesTools = new FabrikamSalesTools(httpClient, configuration);

        var ordersDto = new List<OrderDetailDto>
        {
            new() { 
                Id = 1, 
                Status = status, 
                Financials = new OrderFinancialDto { Total = 1000.00m },
                Customer = new OrderCustomerDto { Name = "Test Customer" },
                Region = "North"
            }
        };

        var jsonResponse = JsonSerializer.Serialize(ordersDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        // Act
        var result = await salesTools.GetOrders(status: status);

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain(status);
    }
}
