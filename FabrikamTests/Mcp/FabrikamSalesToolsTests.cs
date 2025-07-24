using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using FabrikamMcp.Tools;
using FabrikamApi.DTOs;

namespace FabrikamTests.Mcp;

[Trait("Category", "Mcp")]
public class FabrikamSalesToolsTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<FabrikamSalesTools>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public FabrikamSalesToolsTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<FabrikamSalesTools>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClient);
    }

    [Fact]
    public async Task GetSalesAnalytics_WithValidResponse_ReturnsStructuredData()
    {
        // Arrange
        var analyticsDto = new SalesAnalyticsDto
        {
            Summary = new SalesAnalyticsDto.SalesSummaryDto
            {
                TotalOrders = 10,
                TotalRevenue = 50000,
                AverageOrderValue = 5000,
                Period = new SalesAnalyticsDto.PeriodDto
                {
                    FromDate = "2025-01-01",
                    ToDate = "2025-01-31"
                }
            },
            ByStatus = new List<SalesAnalyticsDto.StatusBreakdownDto>
            {
                new() { Status = "Completed", Count = 8, Revenue = 40000 },
                new() { Status = "Pending", Count = 2, Revenue = 10000 }
            },
            ByRegion = new List<SalesAnalyticsDto.RegionBreakdownDto>
            {
                new() { Region = "North", Count = 6, Revenue = 30000 },
                new() { Region = "South", Count = 4, Revenue = 20000 }
            },
            RecentTrends = new List<SalesAnalyticsDto.TrendDataDto>
            {
                new() { Date = "2025-01-01", Orders = 5, Revenue = 25000 },
                new() { Date = "2025-01-02", Orders = 5, Revenue = 25000 }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(analyticsDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClientFactoryMock.Object, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics();

        // Assert
        result.Should().NotBeNull();
        
        // Convert result to string and check it contains expected MCP structure
        var resultString = result.ToString();
        resultString.Should().Contain("content");
        resultString.Should().Contain("analyticsData");
        resultString.Should().Contain("outputSchema");
    }

    [Fact]
    public async Task GetSalesAnalytics_WithApiError_ReturnsErrorResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal Server Error"),
            ReasonPhrase = "Internal Server Error"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClientFactoryMock.Object, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("error");
        resultString.Should().Contain("500");
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
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClientFactoryMock.Object, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await salesTools.GetSalesAnalytics();

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("error");
        resultString.Should().Contain("Connection failed");
    }

    [Theory]
    [InlineData("2025-01-01", "2025-01-31")]
    [InlineData(null, null)]
    [InlineData("2025-01-01", null)]
    [InlineData(null, "2025-01-31")]
    public async Task GetSalesAnalytics_WithDateParameters_CallsCorrectUrl(string? fromDate, string? toDate)
    {
        // Arrange
        var analyticsDto = new SalesAnalyticsDto
        {
            Summary = new SalesAnalyticsDto.SalesSummaryDto { TotalOrders = 1, TotalRevenue = 1000, AverageOrderValue = 1000 },
            ByStatus = new List<SalesAnalyticsDto.StatusBreakdownDto>(),
            ByRegion = new List<SalesAnalyticsDto.RegionBreakdownDto>(),
            RecentTrends = new List<SalesAnalyticsDto.TrendDataDto>()
        };

        var jsonResponse = JsonSerializer.Serialize(analyticsDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        HttpRequestMessage? capturedRequest = null;
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(httpResponse);

        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var salesTools = new FabrikamSalesTools(_httpClientFactoryMock.Object, _configurationMock.Object, _loggerMock.Object);

        // Act
        await salesTools.GetSalesAnalytics(fromDate, toDate);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri.Should().NotBeNull();
        
        var requestUrl = capturedRequest.RequestUri!.ToString();
        requestUrl.Should().StartWith("http://localhost:5235/api/orders/analytics");
        
        if (!string.IsNullOrEmpty(fromDate))
        {
            requestUrl.Should().Contain($"fromDate={Uri.EscapeDataString(fromDate)}");
        }
        
        if (!string.IsNullOrEmpty(toDate))
        {
            requestUrl.Should().Contain($"toDate={Uri.EscapeDataString(toDate)}");
        }
    }
}
