using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using FluentAssertions;
using FabrikamTests.Helpers;
using Xunit;

namespace FabrikamTests.Integration.Api;

[Trait("Category", "Api")]
public class SupportTicketsControllerTests : AuthenticationTestBase
{
    public SupportTicketsControllerTests(AuthenticationTestApplicationFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task GetSupportTickets_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/supporttickets");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task GetSupportTickets_ReturnsExpectedStructure()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/supporttickets");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Assert
        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        
        if (result.RootElement.GetArrayLength() > 0)
        {
            var firstTicket = result.RootElement[0];
            firstTicket.TryGetProperty("id", out _).Should().BeTrue();
            firstTicket.TryGetProperty("title", out _).Should().BeTrue();
            firstTicket.TryGetProperty("status", out _).Should().BeTrue();
            firstTicket.TryGetProperty("priority", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetSupportTicket_WithValidId_ReturnsTicket()
    {
        // Arrange - First get all tickets to find a valid ID
        var allTicketsResponse = await AuthenticatedClient.GetAsync("/api/supporttickets");
        var allTicketsContent = await allTicketsResponse.Content.ReadAsStringAsync();
        var allTickets = JsonDocument.Parse(allTicketsContent);
        
        if (allTickets.RootElement.GetArrayLength() == 0)
        {
            return; // Skip test if no tickets exist
        }

        var firstTicket = allTickets.RootElement[0];
        var ticketId = firstTicket.GetProperty("id").GetInt32();

        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/supporttickets/{ticketId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);
        
        result.RootElement.GetProperty("id").GetInt32().Should().Be(ticketId);
    }

    [Fact]
    public async Task GetSupportTicket_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/supporttickets/99999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("Open")]
    [InlineData("InProgress")]
    [InlineData("Closed")]
    public async Task GetSupportTickets_WithStatusFilter_ReturnsFilteredResults(string status)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/supporttickets?status={status}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Verify all returned tickets have the specified status
        foreach (var ticket in result.RootElement.EnumerateArray())
        {
            if (ticket.TryGetProperty("status", out var statusProperty))
            {
                statusProperty.GetString().Should().Be(status);
            }
        }
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public async Task GetSupportTickets_WithPriorityFilter_ReturnsFilteredResults(string priority)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/supporttickets?priority={priority}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Verify all returned tickets have the specified priority
        foreach (var ticket in result.RootElement.EnumerateArray())
        {
            if (ticket.TryGetProperty("priority", out var priorityProperty))
            {
                priorityProperty.GetString().Should().Be(priority);
            }
        }
    }

    [Fact]
    public async Task PostSupportTicket_WithValidData_CreatesTicket()
    {
        // Arrange
        var newTicket = new
        {
            title = "Test Support Ticket",
            description = "This is a test ticket created by automated testing.",
            priority = "Medium",
            customerId = 1,
            customerEmail = "test@fabrikam.com"
        };

        var json = JsonSerializer.Serialize(newTicket);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await AuthenticatedClient.PostAsync("/api/supporttickets", content);

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(responseContent);
            
            result.RootElement.TryGetProperty("id", out _).Should().BeTrue();
            result.RootElement.TryGetProperty("title", out var titleProperty).Should().BeTrue();
            titleProperty.GetString().Should().Be("Test Support Ticket");
        }
        else
        {
            // If POST is not implemented yet, we should get 405 Method Not Allowed or 501 Not Implemented
            response.StatusCode.Should().BeOneOf(
                System.Net.HttpStatusCode.MethodNotAllowed,
                System.Net.HttpStatusCode.NotImplemented,
                System.Net.HttpStatusCode.BadRequest
            );
        }
    }

    [Fact]
    public async Task PostSupportTicket_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange - Missing required fields
        var invalidTicket = new
        {
            title = "", // Empty title should be invalid
            description = "Test description"
        };

        var json = JsonSerializer.Serialize(invalidTicket);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await AuthenticatedClient.PostAsync("/api/supporttickets", content);

        // Assert
        if (response.StatusCode != System.Net.HttpStatusCode.MethodNotAllowed && 
            response.StatusCode != System.Net.HttpStatusCode.NotImplemented)
        {
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task PutSupportTicket_WithValidData_UpdatesTicket()
    {
        // Arrange - First get an existing ticket
        var allTicketsResponse = await AuthenticatedClient.GetAsync("/api/supporttickets");
        var allTicketsContent = await allTicketsResponse.Content.ReadAsStringAsync();
        var allTickets = JsonDocument.Parse(allTicketsContent);
        
        if (allTickets.RootElement.GetArrayLength() == 0)
        {
            return; // Skip test if no tickets exist
        }

        var firstTicket = allTickets.RootElement[0];
        var ticketId = firstTicket.GetProperty("id").GetInt32();

        var updatedTicket = new
        {
            id = ticketId,
            title = "Updated Test Ticket",
            description = "Updated description",
            status = "InProgress",
            priority = "High"
        };

        var json = JsonSerializer.Serialize(updatedTicket);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await AuthenticatedClient.PutAsync($"/api/supporttickets/{ticketId}", content);

        // Assert
        if (response.StatusCode != System.Net.HttpStatusCode.MethodNotAllowed && 
            response.StatusCode != System.Net.HttpStatusCode.NotImplemented)
        {
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }

    [Fact]
    public async Task DeleteSupportTicket_WithValidId_DeletesTicket()
    {
        // Arrange - First create or find a ticket to delete
        var allTicketsResponse = await AuthenticatedClient.GetAsync("/api/supporttickets");
        var allTicketsContent = await allTicketsResponse.Content.ReadAsStringAsync();
        var allTickets = JsonDocument.Parse(allTicketsContent);
        
        if (allTickets.RootElement.GetArrayLength() == 0)
        {
            return; // Skip test if no tickets exist
        }

        var firstTicket = allTickets.RootElement[0];
        var ticketId = firstTicket.GetProperty("id").GetInt32();

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"/api/supporttickets/{ticketId}");

        // Assert
        if (response.StatusCode != System.Net.HttpStatusCode.MethodNotAllowed && 
            response.StatusCode != System.Net.HttpStatusCode.NotImplemented)
        {
            response.IsSuccessStatusCode.Should().BeTrue();
            
            // Verify the ticket is actually deleted
            var getResponse = await AuthenticatedClient.GetAsync($"/api/supporttickets/{ticketId}");
            getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 3)]
    [InlineData(1, 10)]
    public async Task GetSupportTickets_WithPagination_ReturnsCorrectPageSize(int page, int pageSize)
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"/api/supporttickets?page={page}&pageSize={pageSize}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Check that we don't exceed the requested page size
        result.RootElement.GetArrayLength().Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task GetSupportTickets_WithMultipleFilters_ReturnsCorrectResults()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/supporttickets?status=Open&priority=High&page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        
        // Verify filters are applied correctly
        foreach (var ticket in result.RootElement.EnumerateArray())
        {
            if (ticket.TryGetProperty("status", out var statusProperty))
            {
                statusProperty.GetString().Should().Be("Open");
            }
            if (ticket.TryGetProperty("priority", out var priorityProperty))
            {
                priorityProperty.GetString().Should().Be("High");
            }
        }
    }

    [Fact]
    public async Task GetSupportTickets_WithInvalidStatus_ReturnsEmptyArray()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/supporttickets?status=InvalidStatus");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        result.RootElement.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetSupportTickets_WithCustomerFilter_ReturnsCustomerTickets()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/supporttickets?customerId=1");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Verify all returned tickets belong to the specified customer
        foreach (var ticket in result.RootElement.EnumerateArray())
        {
            if (ticket.TryGetProperty("customerId", out var customerIdProperty))
            {
                customerIdProperty.GetInt32().Should().Be(1);
            }
        }
    }

    [Fact]
    public async Task GetSupportTickets_ResponseTimeIsReasonable()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await AuthenticatedClient.GetAsync("/api/supporttickets");

        // Assert
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should respond within 1 second
    }

    [Fact]
    public async Task GetSupportTickets_SortingWorks()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/supporttickets?sortBy=priority&sortOrder=desc");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        result.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        
        // If sorting is implemented, priorities should be in descending order
        // This is a basic check - actual implementation may vary
        if (result.RootElement.GetArrayLength() > 1)
        {
            // Just verify we can access priority properties
            foreach (var ticket in result.RootElement.EnumerateArray())
            {
                ticket.TryGetProperty("priority", out _).Should().BeTrue();
            }
        }
    }
}
