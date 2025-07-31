using System.Net;
using System.Text.Json;
using FluentAssertions;
using FabrikamTests.Helpers;
using FabrikamContracts.DTOs;
using Xunit;

namespace FabrikamTests.Integration.Api;

/// <summary>
/// Debugging test to investigate authentication infrastructure issues
/// </summary>
[Trait("Category", "Debug")]
public class AuthenticationDebugTests : AuthenticationTestBase
{
    public AuthenticationDebugTests(AuthenticationTestApplicationFactory factory) 
        : base(factory)
    {
        // Test with environment defaults (should be Disabled in Testing environment)
    }

    [Fact]
    public async Task Debug_GetCustomers_CheckResponse()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync("/api/customers");

        // Debug output
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();
        
        // Print debug information
        Console.WriteLine($"Status Code: {statusCode}");
        Console.WriteLine($"Content: {content}");
        Console.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
        
        // Check what we got
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Request failed with {statusCode}");
            Console.WriteLine($"Response content: {content}");
        }

        // Assert for now just to see what happens
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError, 
            "Should not have server errors");
    }

    [Fact]
    public async Task Debug_GetInfo_NoAuth()
    {
        // Act - Test an endpoint that might not require auth
        var response = await Client.GetAsync("/api/info");

        // Debug output
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"Info endpoint - Status Code: {statusCode}");
        Console.WriteLine($"Info endpoint - Content: {content}");
        
        // This should work regardless of auth
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Debug_CheckJwtToken()
    {
        // Debug our JWT token
        Console.WriteLine($"JWT Token: {ValidJwtToken}");
        Console.WriteLine($"Token length: {ValidJwtToken.Length}");
        Console.WriteLine($"User ID: {GetCurrentUserId()}");
        Console.WriteLine($"Username: {GetCurrentUsername()}");
        
        // Basic validation
        ValidJwtToken.Should().NotBeNullOrEmpty();
        ValidJwtToken.Should().Contain(".");
    }
}
