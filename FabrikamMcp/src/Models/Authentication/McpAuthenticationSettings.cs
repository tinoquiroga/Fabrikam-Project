using FabrikamContracts.DTOs;

namespace FabrikamMcp.Models.Authentication;

/// <summary>
/// MCP-specific authentication configuration settings
/// Separate from API authentication to allow independent configuration
/// </summary>
public class McpAuthenticationSettings : FabrikamContracts.DTOs.AuthenticationSettings
{
    /// <summary>
    /// Configuration section name for MCP authentication settings
    /// </summary>
    public new const string SectionName = "McpAuthentication";
    
    /// <summary>
    /// Service JWT settings specific to MCP
    /// </summary>
    public new ServiceJwtSettings ServiceJwt { get; set; } = new();
    
    /// <summary>
    /// GUID validation settings for Disabled authentication mode
    /// </summary>
    public new GuidValidationSettings GuidValidation { get; set; } = new();
}
