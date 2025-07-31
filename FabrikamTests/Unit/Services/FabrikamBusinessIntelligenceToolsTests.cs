using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using FabrikamMcp.Tools;
using FabrikamMcp.Services;
using Xunit;

namespace FabrikamTests.Unit.Services;

[Trait("Category", "Mcp")]
public class FabrikamBusinessIntelligenceToolsTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<ILogger<FabrikamBusinessIntelligenceTools>> _loggerMock;
    private readonly HttpClient _httpClient;

    public FabrikamBusinessIntelligenceToolsTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<FabrikamBusinessIntelligenceTools>>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        
        // Setup default authentication service behavior
        _authServiceMock.Setup(x => x.IsAuthenticated()).Returns(true);
        _authServiceMock.Setup(x => x.GetCurrentJwtToken()).Returns("mock-jwt-token");
        _authServiceMock.Setup(x => x.GetCurrentJwtTokenAsync()).ReturnsAsync("mock-jwt-token");
    }

    private FabrikamBusinessIntelligenceTools CreateBusinessTools()
    {
        return new FabrikamBusinessIntelligenceTools(_httpClient, _configurationMock.Object, _authServiceMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("30days")]
    [InlineData("7days")]
    [InlineData("year")]
    [InlineData("quarter")]
    [InlineData("month")]
    public async Task GetBusinessDashboard_WithValidTimeframes_ReturnsStructuredData(string timeframe)
    {
        // Arrange
        SetupMockResponses();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var businessTools = CreateBusinessTools();

        // Act
        var result = await businessTools.GetBusinessDashboard(timeframe);

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("content");
        resultString.Should().Contain("data");
        resultString.Should().Contain(timeframe);
    }

    [Fact]
    public async Task GetBusinessDashboard_WithIncludeForecasts_ReturnsStructuredData()
    {
        // Arrange
        SetupMockResponses();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var businessTools = CreateBusinessTools();

        // Act
        var result = await businessTools.GetBusinessDashboard("30days", includeForecasts: true);

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("content");
        resultString.Should().Contain("data");
        resultString.Should().Contain("30days");
    }

    [Fact]
    public async Task GetBusinessDashboard_WithApiErrors_HandlesGracefully()
    {
        // Arrange
        SetupMockErrorResponses();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var businessTools = CreateBusinessTools();

        // Act
        var result = await businessTools.GetBusinessDashboard("30days");

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        // Business dashboard continues to return content even with API errors
        resultString.Should().Contain("content");
        resultString.Should().Contain("data");
    }

    [Fact]
    public async Task GetBusinessDashboard_WithHttpException_ReturnsErrorResponse()
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

        var businessTools = CreateBusinessTools();

        // Act
        var result = await businessTools.GetBusinessDashboard("30days");

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        resultString.Should().Contain("error");
        resultString.Should().Contain("Connection failed");
    }

    [Fact]
    public async Task GetBusinessDashboard_WithEmptyApiResponses_HandlesGracefully()
    {
        // Arrange
        SetupMockEmptyResponses();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var businessTools = CreateBusinessTools();

        // Act
        var result = await businessTools.GetBusinessDashboard("30days");

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        // Should not throw parsing errors even with empty/null responses
        resultString.Should().Contain("content");
    }

    [Fact]
    public async Task GetBusinessDashboard_WithMalformedJson_HandlesGracefully()
    {
        // Arrange
        SetupMockMalformedResponses();
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var businessTools = CreateBusinessTools();

        // Act
        var result = await businessTools.GetBusinessDashboard("30days");

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        // Should return error response for malformed JSON
        resultString.Should().Contain("error");
    }

    [Fact]
    public async Task GetBusinessDashboard_WithYearTimeframe_NoParsingErrors()
    {
        // This is a specific test for the issue we fixed - "year" timeframe should not cause parsing errors

        // Arrange
        SetupMockEmptyResponses(); // Use empty responses to test the safe parsing
        _configurationMock.Setup(x => x["FabrikamApi:BaseUrl"]).Returns("http://localhost:5235");

        var businessTools = CreateBusinessTools();

        // Act
        var result = await businessTools.GetBusinessDashboard("year");

        // Assert
        result.Should().NotBeNull();
        var resultString = result.ToString();
        // Should not contain any parsing error messages
        resultString.Should().NotContain("format");
        resultString.Should().NotContain("parse");
        resultString.Should().Contain("content");
    }

    private void SetupMockResponses()
    {
        var queue = new Queue<HttpResponseMessage>();

        // Sales analytics response
        var salesAnalytics = new
        {
            summary = new
            {
                totalOrders = 125,
                totalRevenue = 487500.75m,
                averageOrderValue = 3900.00m,
                period = new { fromDate = "2025-01-01", toDate = "2025-01-31" }
            },
            byStatus = new[]
            {
                new { status = "Completed", count = 100, revenue = 390000.50m },
                new { status = "Pending", count = 25, revenue = 97500.25m }
            },
            byRegion = new[]
            {
                new { region = "North", count = 75, revenue = 292500.45m },
                new { region = "South", count = 50, revenue = 195000.30m }
            }
        };

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(salesAnalytics), System.Text.Encoding.UTF8, "application/json")
        });

        // Support analytics response
        var supportAnalytics = new
        {
            summary = new
            {
                totalTickets = 45,
                averageResolutionTime = 24.5,
                satisfactionScore = 4.2
            },
            byPriority = new[]
            {
                new { priority = "High", count = 5, avgResolutionHours = 12.5 },
                new { priority = "Medium", count = 25, avgResolutionHours = 24.0 },
                new { priority = "Low", count = 15, avgResolutionHours = 48.0 }
            }
        };

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(supportAnalytics), System.Text.Encoding.UTF8, "application/json")
        });

        // Products response
        var products = new[]
        {
            new { id = 1, name = "Test Product 1", stockQuantity = 50, price = 1500.00m },
            new { id = 2, name = "Test Product 2", stockQuantity = 0, price = 2500.00m }
        };

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(products), System.Text.Encoding.UTF8, "application/json")
        });

        // Orders response
        var orders = new[]
        {
            new { id = 1, status = "Completed", total = 3900.00m, orderDate = "2025-01-15T10:30:00Z" },
            new { id = 2, status = "Pending", total = 2500.00m, orderDate = "2025-01-20T14:15:00Z" }
        };

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(orders), System.Text.Encoding.UTF8, "application/json")
        });

        // Support tickets response
        var tickets = new[]
        {
            new { id = 1, priority = "High", status = "Open", subject = "Critical Issue" },
            new { id = 2, priority = "Medium", status = "Closed", subject = "General Question" }
        };

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(tickets), System.Text.Encoding.UTF8, "application/json")
        });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => queue.Count > 0 ? queue.Dequeue() : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
    }

    private void SetupMockErrorResponses()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal Server Error"),
                ReasonPhrase = "Internal Server Error"
            });
    }

    private void SetupMockEmptyResponses()
    {
        var queue = new Queue<HttpResponseMessage>();

        // Empty responses with null/empty values to test parsing robustness
        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"summary\":{\"totalOrders\":\"\",\"totalRevenue\":\"\",\"averageOrderValue\":\"\"}}", System.Text.Encoding.UTF8, "application/json")
        });

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"summary\":{\"totalTickets\":\"\",\"averageResolutionTime\":\"\",\"satisfactionScore\":\"\"}}", System.Text.Encoding.UTF8, "application/json")
        });

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
        });

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
        });

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
        });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => queue.Count > 0 ? queue.Dequeue() : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
    }

    private void SetupMockMalformedResponses()
    {
        var queue = new Queue<HttpResponseMessage>();

        // Malformed JSON responses to test error handling
        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid json", System.Text.Encoding.UTF8, "application/json")
        });

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json at all", System.Text.Encoding.UTF8, "application/json")
        });

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"summary\":null}", System.Text.Encoding.UTF8, "application/json")
        });

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        });

        queue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => queue.Count > 0 ? queue.Dequeue() : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
    }
}

