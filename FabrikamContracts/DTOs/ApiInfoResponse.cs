using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FabrikamContracts.DTOs;

/// <summary>
/// API information response structure
/// This DTO defines the official API contract for the InfoController
/// </summary>
public class ApiInfoResponse
{
    /// <summary>
    /// Application name - serialized as 'applicationName' in JSON
    /// </summary>
    [JsonPropertyName("applicationName")]
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Application version - serialized as 'version' in JSON
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Build date - serialized as 'buildDate' in JSON
    /// </summary>
    [JsonPropertyName("buildDate")]
    public string BuildDate { get; set; } = string.Empty;

    /// <summary>
    /// Current environment - serialized as 'environment' in JSON
    /// </summary>
    [JsonPropertyName("environment")]
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Database status - serialized as 'databaseStatus' in JSON
    /// </summary>
    [JsonPropertyName("databaseStatus")]
    public string DatabaseStatus { get; set; } = string.Empty;

    /// <summary>
    /// Authentication configuration - serialized as 'authenticationConfiguration' in JSON
    /// </summary>
    [JsonPropertyName("authenticationConfiguration")]
    public object? AuthenticationConfiguration { get; set; }
}