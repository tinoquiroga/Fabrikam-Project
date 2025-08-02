using FabrikamContracts.DTOs;
using FabrikamApi.Models.Authentication;

namespace FabrikamApi.Models.Authentication;

/// <summary>
/// API-specific authentication configuration settings
/// Separate from MCP authentication to allow independent configuration
/// </summary>
public class ApiAuthenticationSettings : FabrikamContracts.DTOs.AuthenticationSettings
{
    /// <summary>
    /// Configuration section name for API authentication settings
    /// </summary>
    public new const string SectionName = "ApiAuthentication";
    
    /// <summary>
    /// ASP.NET Identity specific settings for API
    /// </summary>
    public AspNetIdentitySettings AspNetIdentity { get; set; } = new();
}
