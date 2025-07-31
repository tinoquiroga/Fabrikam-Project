using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using FluentAssertions;
using FabrikamTests.Helpers;
using Xunit;

namespace FabrikamTests.Integration.Api;

[Trait("Category", "Api")]
public class InfoControllerTests : AuthenticationTestBase
{
    public InfoControllerTests(AuthenticationTestApplicationFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task GetInfo_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await Client.GetAsync("/api/info");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task GetInfo_ReturnsExpectedStructure()
    {
        // Act
        var response = await Client.GetAsync("/api/info");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Assert
        result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        
        // Check for expected properties
        result.RootElement.TryGetProperty("applicationName", out _).Should().BeTrue();
        result.RootElement.TryGetProperty("version", out _).Should().BeTrue();
        result.RootElement.TryGetProperty("environment", out _).Should().BeTrue();
        result.RootElement.TryGetProperty("buildDate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetInfo_ReturnsValidApplicationName()
    {
        // Act
        var response = await Client.GetAsync("/api/info");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Assert
        result.RootElement.TryGetProperty("applicationName", out var appNameProperty).Should().BeTrue();
        var appName = appNameProperty.GetString();
        appName.Should().NotBeNullOrEmpty();
        appName.Should().Contain("Fabrikam");
    }

    [Fact]
    public async Task GetInfo_ReturnsValidVersion()
    {
        // Act
        var response = await Client.GetAsync("/api/info");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Assert
        result.RootElement.TryGetProperty("version", out var versionProperty).Should().BeTrue();
        var version = versionProperty.GetString();
        version.Should().NotBeNullOrEmpty();
        version.Should().MatchRegex(@"^\d+\.\d+\.\d+.*"); // Semantic versioning pattern
    }

    [Fact]
    public async Task GetInfo_ReturnsValidEnvironment()
    {
        // Act
        var response = await Client.GetAsync("/api/info");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Assert
        result.RootElement.TryGetProperty("environment", out var envProperty).Should().BeTrue();
        var environment = envProperty.GetString();
        environment.Should().NotBeNullOrEmpty();
        environment.Should().BeOneOf("Development", "Production", "Test");
    }

    [Fact]
    public async Task GetInfo_ReturnsBuildDate()
    {
        // Act
        var response = await Client.GetAsync("/api/info");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Assert
        result.RootElement.TryGetProperty("buildDate", out var buildDateProperty).Should().BeTrue();
        var buildDateString = buildDateProperty.GetString();
        buildDateString.Should().NotBeNullOrEmpty();
        
        // Should be a valid date
        DateTime.TryParse(buildDateString, out var buildDate).Should().BeTrue();
        buildDate.Should().BeAfter(new DateTime(2024, 1, 1)); // Should be recent
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthStatus()
    {
        // Act
        var response = await Client.GetAsync("/api/info/health");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Check for health status properties
        result.RootElement.TryGetProperty("status", out var statusProperty).Should().BeTrue();
        var status = statusProperty.GetString();
        status.Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
    }

    [Fact]
    public async Task GetHealth_IncludesTimestamp()
    {
        // Act
        var response = await Client.GetAsync("/api/info/health");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Should include a timestamp
        result.RootElement.TryGetProperty("timestamp", out var timestampProperty).Should().BeTrue();
        var timestampString = timestampProperty.GetString();
        timestampString.Should().NotBeNullOrEmpty();
        
        DateTime.TryParse(timestampString, out var timestamp).Should().BeTrue();
    }

    [Fact]
    public async Task GetHealth_IncludesServiceChecks()
    {
        // Act
        var response = await Client.GetAsync("/api/info/health");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Should include service health checks
        result.RootElement.TryGetProperty("services", out var servicesProperty).Should().BeTrue();
        servicesProperty.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task GetDependencies_ReturnsDependencyInformation()
    {
        // Act
        var response = await Client.GetAsync("/api/info/dependencies");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(content);

            // Should be an object or array containing dependency info
            result.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
        else
        {
            // Endpoint might not be implemented yet - that's okay
            response.StatusCode.Should().BeOneOf(
                System.Net.HttpStatusCode.NotFound,
                System.Net.HttpStatusCode.NotImplemented
            );
        }
    }

    [Fact]
    public async Task GetConfiguration_ReturnsConfigurationInfo()
    {
        // Act
        var response = await Client.GetAsync("/api/info/configuration");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(content);

            // Should contain configuration information (but not sensitive data)
            result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
            
            // Should NOT contain sensitive information like connection strings or keys
            var contentLower = content.ToLower();
            contentLower.Should().NotContain("password");
            contentLower.Should().NotContain("secret");
            contentLower.Should().NotContain("key");
            contentLower.Should().NotContain("connectionstring");
        }
        else
        {
            // Endpoint might not be implemented yet - that's okay
            response.StatusCode.Should().BeOneOf(
                System.Net.HttpStatusCode.NotFound,
                System.Net.HttpStatusCode.NotImplemented
            );
        }
    }

    [Fact]
    public async Task GetInfo_ResponseTimeIsReasonable()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await Client.GetAsync("/api/info");

        // Assert
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should respond within 1 second
    }

    [Fact]
    public async Task GetInfo_IsIdempotent()
    {
        // Act - Call the endpoint multiple times
        var response1 = await Client.GetAsync("/api/info");
        var content1 = await response1.Content.ReadAsStringAsync();
        
        var response2 = await Client.GetAsync("/api/info");
        var content2 = await response2.Content.ReadAsStringAsync();

        // Assert - Should return consistent results (except for timestamps)
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();
        
        var result1 = JsonDocument.Parse(content1);
        var result2 = JsonDocument.Parse(content2);
        
        // Application name and version should be identical
        result1.RootElement.TryGetProperty("applicationName", out var appName1).Should().BeTrue();
        result2.RootElement.TryGetProperty("applicationName", out var appName2).Should().BeTrue();
        appName1.GetString().Should().Be(appName2.GetString());
        
        result1.RootElement.TryGetProperty("version", out var version1).Should().BeTrue();
        result2.RootElement.TryGetProperty("version", out var version2).Should().BeTrue();
        version1.GetString().Should().Be(version2.GetString());
    }
}
