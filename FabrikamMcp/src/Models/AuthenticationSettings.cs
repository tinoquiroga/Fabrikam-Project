using FabrikamContracts.DTOs;

namespace FabrikamMcp.Models;

// Import consolidated authentication models from contracts for compatibility
using AuthenticationMode = FabrikamContracts.DTOs.AuthenticationMode;
using AuthenticationSettings = FabrikamContracts.DTOs.AuthenticationSettings;
using AuthenticationContext = FabrikamContracts.DTOs.AuthenticationContext;
using JwtSettings = FabrikamContracts.DTOs.JwtSettings;
using ServiceJwtSettings = FabrikamContracts.DTOs.ServiceJwtSettings;
using GuidValidationSettings = FabrikamContracts.DTOs.GuidValidationSettings;
using EntraExternalIdSettings = FabrikamContracts.DTOs.EntraExternalIdSettings;

/// <summary>
/// MCP-specific authentication extensions
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Validates authentication settings with MCP-specific requirements
    /// </summary>
    public static void ValidateForMcp(this AuthenticationSettings settings)
    {
        settings.Validate(); // Use base validation from contracts
        
        // Add any MCP-specific validation rules here if needed
        if (settings.Mode == AuthenticationMode.Disabled && !settings.GuidValidation.Enabled)
        {
            throw new InvalidOperationException("GUID validation must be enabled for Disabled authentication mode in MCP");
        }
    }
}
