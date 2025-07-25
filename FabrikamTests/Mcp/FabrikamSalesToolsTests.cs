using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using FabrikamMcp.Tools;
using FabrikamContracts.DTOs.Orders;
using Xunit;

namespace FabrikamTests.Mcp;

[Trait("Category", "Mcp")]
public class FabrikamSalesToolsTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public FabrikamSalesToolsTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
    }

    [Fact]
    public async Task GetSalesAnalytics_WithValidResponse_ReturnsStructuredData()
    {
        // Arrange
        SetupMockAnalyticsResponse();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClient, _configurationMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("content");
        resultString.Should().Contain("analyticsData");
        resultString.Should().Contain("TotalOrders");
        resultString.Should().Contain("TotalRevenue");
    }

    [Fact]
    public async Task GetSalesAnalytics_WithDateRange_ReturnsFilteredData()
    {
        // Arrange
        SetupMockAnalyticsResponse();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClient, _configurationMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics("2024-01-01", "2024-01-31");

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("content");
        resultString.Should().Contain("2024-01-01");
        resultString.Should().Contain("2024-01-31");
    }

    [Fact]
    public async Task GetSalesAnalytics_WithValidResponse_ReturnsRegionalData()
    {
        // Arrange
        SetupMockAnalyticsResponse();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClient, _configurationMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("content");
        resultString.Should().Contain("analyticsData");
    }

    [Fact]
    public async Task GetSalesAnalytics_WithApiError_HandlesGracefully()
    {
        // Arrange
        SetupMockErrorResponse();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClient, _configurationMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("error");
    }

    [Fact]
    public async Task GetSalesAnalytics_WithHttpException_ReturnsErrorResponse()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClient, _configurationMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("error");
        resultString.Should().Contain("Network error");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-date")]
    [InlineData("2024-13-01")] // Invalid month
    public async Task GetSalesAnalytics_WithInvalidDates_HandlesGracefully(string invalidDate)
    {
        // Arrange
        SetupMockAnalyticsResponse();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClient, _configurationMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics(invalidDate, "2024-01-31");

        // Assert
        result.Should().NotBeNull();
        // Should handle gracefully and still return content
        var resultString = result.ToString();
        resultString.Should().Contain("content");
    }

    [Fact]
    public async Task GetCustomers_WithValidResponse_ReturnsStructuredData()
    {
        // Arrange
        SetupMockCustomerResponse();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClient, _configurationMock.Object);

        // Act
        var result = await salesTools.GetCustomers();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("content");
        resultString.Should().Contain("customersData");
    }

    [Fact]
    public async Task GetCustomers_WithRegionFilter_ReturnsFilteredCustomers()
    {
        // Arrange
        SetupMockCustomerResponse();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClient, _configurationMock.Object);

        // Act
        var result = await salesTools.GetCustomers(region: "North");

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("content");
        resultString.Should().Contain("customersData");
    }

    private void SetupMockAnalyticsResponse()
    {
        var mockAnalytics = new
        {
            Summary = new
            {
                TotalOrders = 10,
                TotalRevenue = 50000.00m,
                AverageOrderValue = 5000.00m,
                Period = new
                {
                    FromDate = "2024-01-01",
                    ToDate = "2024-01-31"
                }
            },
            ByStatus = new[]
            {
                new { Status = "Completed", Count = 8, Revenue = 40000.00m },
                new { Status = "Pending", Count = 2, Revenue = 10000.00m }
            },
            ByRegion = new[]
            {
                new { Region = "North", Count = 6, Revenue = 30000.00m },
                new { Region = "South", Count = 4, Revenue = 20000.00m }
            },
            RecentTrends = new[]
            {
                new { Date = "2024-01-01", Orders = 5, Revenue = 25000.00m },
                new { Date = "2024-01-02", Orders = 5, Revenue = 25000.00m }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockAnalytics);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);
    }

    private void SetupMockCustomerResponse()
    {
        var mockCustomers = new[]
        {
            new
            {
                Id = 1,
                Name = "John Smith",
                Email = "john.smith@example.com",
                Region = "North",
                TotalOrders = 3,
                TotalSpent = 15000.00m
            },
            new
            {
                Id = 2,
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                Region = "South",
                TotalOrders = 2,
                TotalSpent = 10000.00m
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockCustomers);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        // Add the X-Total-Count header that the MCP tool expects
        httpResponse.Headers.Add("X-Total-Count", "2");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);
    }

    private void SetupMockErrorResponse()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal server error", System.Text.Encoding.UTF8, "text/plain")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);
    }
}
