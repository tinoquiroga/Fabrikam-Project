using FluentAssertions;
using System.Text.Json;
using System.Net.Http;
using Xunit.Abstractions;

namespace FabrikamTests.Helpers;

/// <summary>
/// Enhanced test base with Given/When/Then pattern support and improved assertions
/// Provides fluent API for test organization and common test operations
/// </summary>
public abstract class GivenWhenThenTestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly JsonSerializerOptions JsonOptions;

    // Test context for storing data between Given/When/Then steps
    protected readonly Dictionary<string, object> TestContext = new();

    protected GivenWhenThenTestBase(ITestOutputHelper output)
    {
        Output = output;
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Given Methods (Test Setup)

    /// <summary>
    /// Start a Given statement for test setup
    /// </summary>
    protected GivenStatement Given => new(this);

    /// <summary>
    /// Store data in test context for use in When/Then
    /// </summary>
    protected void StoreInContext<T>(string key, T value)
    {
        TestContext[key] = value!;
    }

    /// <summary>
    /// Retrieve data from test context
    /// </summary>
    protected T GetFromContext<T>(string key)
    {
        return TestContext.TryGetValue(key, out var value) ? (T)value : default!;
    }

    #endregion

    #region When Methods (Test Actions)

    /// <summary>
    /// Start a When statement for test actions
    /// </summary>
    protected WhenStatement When => new(this);

    #endregion

    #region Then Methods (Test Assertions)

    /// <summary>
    /// Start a Then statement for test assertions
    /// </summary>
    protected ThenStatement Then => new(this);

    #endregion

    #region Helper Methods

    /// <summary>
    /// Log test step information
    /// </summary>
    public void LogStep(string step, string description)
    {
        Output.WriteLine($"[{step}] {description}");
    }

    /// <summary>
    /// Deserialize JSON response to typed object
    /// </summary>
    protected T DeserializeResponse<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
        }
        catch (JsonException ex)
        {
            Output.WriteLine($"JSON Deserialization failed: {ex.Message}");
            Output.WriteLine($"JSON Content: {json}");
            throw;
        }
    }

    /// <summary>
    /// Assert HTTP response is successful
    /// </summary>
    protected void AssertSuccessResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            Output.WriteLine($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
        }
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    #endregion

    public virtual void Dispose()
    {
        TestContext.Clear();
    }
}

/// <summary>
/// Fluent Given statement for test setup
/// </summary>
public class GivenStatement
{
    private readonly GivenWhenThenTestBase _testBase;

    public GivenStatement(GivenWhenThenTestBase testBase)
    {
        _testBase = testBase;
    }

    public GivenStatement That(string description, Action setupAction)
    {
        _testBase.LogStep("GIVEN", description);
        setupAction();
        return this;
    }

    public GivenStatement And(string description, Action setupAction)
    {
        return That(description, setupAction);
    }

    public async Task<GivenStatement> ThatAsync(string description, Func<Task> setupAction)
    {
        _testBase.LogStep("GIVEN", description);
        await setupAction();
        return this;
    }

    public async Task<GivenStatement> AndAsync(string description, Func<Task> setupAction)
    {
        return await ThatAsync(description, setupAction);
    }
}

/// <summary>
/// Fluent When statement for test actions
/// </summary>
public class WhenStatement
{
    private readonly GivenWhenThenTestBase _testBase;

    public WhenStatement(GivenWhenThenTestBase testBase)
    {
        _testBase = testBase;
    }

    public WhenStatement I(string description, Action action)
    {
        _testBase.LogStep("WHEN", description);
        action();
        return this;
    }

    public WhenStatement And(string description, Action action)
    {
        return I(description, action);
    }

    public async Task<WhenStatement> IAsync(string description, Func<Task> action)
    {
        _testBase.LogStep("WHEN", description);
        await action();
        return this;
    }

    public async Task<WhenStatement> AndAsync(string description, Func<Task> action)
    {
        return await IAsync(description, action);
    }
}

/// <summary>
/// Fluent Then statement for test assertions
/// </summary>
public class ThenStatement
{
    private readonly GivenWhenThenTestBase _testBase;

    public ThenStatement(GivenWhenThenTestBase testBase)
    {
        _testBase = testBase;
    }

    public ThenStatement The(string description, Action assertion)
    {
        _testBase.LogStep("THEN", description);
        assertion();
        return this;
    }

    public ThenStatement And(string description, Action assertion)
    {
        return The(description, assertion);
    }

    public async Task<ThenStatement> TheAsync(string description, Func<Task> assertion)
    {
        _testBase.LogStep("THEN", description);
        await assertion();
        return this;
    }

    public async Task<ThenStatement> AndAsync(string description, Func<Task> assertion)
    {
        return await TheAsync(description, assertion);
    }
}

/// <summary>
/// Extensions for common test assertions
/// </summary>
public static class TestAssertionExtensions
{
    public static void ShouldBeSuccessful(this HttpResponseMessage response, string? because = null)
    {
        response.IsSuccessStatusCode.Should().BeTrue(because ?? "HTTP response should be successful");
    }

    public static void ShouldHaveStatusCode(this HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode, string? because = null)
    {
        response.StatusCode.Should().Be(expectedStatusCode, because ?? $"HTTP response should have status code {expectedStatusCode}");
    }

    public static void ShouldContainJson(this HttpResponseMessage response, string? because = null)
    {
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json", because ?? "Response should contain JSON");
    }

    public static async Task<T> ShouldDeserializeTo<T>(this HttpResponseMessage response, string? because = null)
    {
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        try
        {
            var result = JsonSerializer.Deserialize<T>(content, options);
            result.Should().NotBeNull(because ?? $"Response should deserialize to {typeof(T).Name}");
            return result!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}: {ex.Message}. Content: {content}");
        }
    }
}
