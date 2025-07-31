using Microsoft.AspNetCore.Mvc.Testing;
using FabrikamApi;
using FabrikamContracts.DTOs;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace FabrikamTests.Helpers;

/// <summary>
/// Smart API test base class that automatically adapts to authentication mode
/// Uses authenticated clients when authentication is enabled, anonymous clients when disabled
/// </summary>
public class SmartApiTestBase : IClassFixture<AuthenticationTestApplicationFactory>
{
    protected readonly AuthenticationTestApplicationFactory Factory;
    protected readonly HttpClient Client;

    /// <summary>
    /// Initialize the test base with optimal client configuration
    /// </summary>
    protected SmartApiTestBase(AuthenticationTestApplicationFactory factory)
    {
        Factory = factory;
        Client = CreateOptimalClient();
    }

    /// <summary>
    /// Create the optimal HTTP client based on current authentication mode
    /// </summary>
    private HttpClient CreateOptimalClient()
    {
        var authSettings = GetAuthenticationSettings();

        // In disabled mode, use anonymous client for simplicity
        if (authSettings.Mode == AuthenticationMode.Disabled)
        {
            return Factory.CreateClient();
        }

        // In authentication-enabled modes, use authenticated client
        return CreateAuthenticatedClient();
    }

    /// <summary>
    /// Create an authenticated client using the standard user token
    /// </summary>
    protected HttpClient CreateAuthenticatedClient()
    {
        var client = Factory.CreateClient();
        var token = JwtTokenHelper.GenerateTestToken();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Create an admin client with admin privileges
    /// </summary>
    protected HttpClient CreateAdminClient()
    {
        var client = Factory.CreateClient();
        var token = JwtTokenHelper.GenerateAdminToken();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Create a manager client with manager privileges
    /// </summary>
    protected HttpClient CreateManagerClient()
    {
        var client = Factory.CreateClient();
        var token = JwtTokenHelper.GenerateManagerToken();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Create an anonymous client (no authentication)
    /// </summary>
    protected HttpClient CreateAnonymousClient()
    {
        return Factory.CreateClient();
    }

    /// <summary>
    /// Get the current authentication settings from the test application
    /// </summary>
    protected AuthenticationSettings GetAuthenticationSettings()
    {
        // For test environments, we expect authentication to be disabled by default
        // unless explicitly overridden in the test factory
        if (Factory.ForceAuthenticationMode)
        {
            return new AuthenticationSettings 
            { 
                Mode = Factory.ForcedAuthenticationMode 
            };
        }

        // Use default environment-aware mode
        return new AuthenticationSettings();
    }

    /// <summary>
    /// Helper method to parse JSON response content
    /// </summary>
    protected async Task<JsonDocument> GetJsonResponseAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }

    /// <summary>
    /// Helper method to check if authentication is required for the current test environment
    /// </summary>
    protected bool IsAuthenticationRequired()
    {
        var authSettings = GetAuthenticationSettings();
        return authSettings.Mode != AuthenticationMode.Disabled;
    }

    /// <summary>
    /// Create a client appropriate for the test scenario
    /// Use this when you want to test with whatever authentication mode is configured
    /// </summary>
    protected HttpClient CreateContextAwareClient()
    {
        return Client; // Already configured optimally in constructor
    }

    /// <summary>
    /// Create a client with a specific authentication mode for testing different scenarios
    /// </summary>
    protected HttpClient CreateClientForMode(AuthenticationMode mode)
    {
        return mode switch
        {
            AuthenticationMode.Disabled => CreateAnonymousClient(),
            AuthenticationMode.BearerToken => CreateAuthenticatedClient(),
            AuthenticationMode.EntraExternalId => CreateAuthenticatedClient(), // Use JWT for now
            _ => throw new ArgumentException($"Unsupported authentication mode: {mode}")
        };
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public virtual void Dispose()
    {
        Client?.Dispose();
    }
}
