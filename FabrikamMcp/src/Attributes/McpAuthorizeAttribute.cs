using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;

namespace FabrikamMcp.Attributes;

/// <summary>
/// Authorization attribute for MCP tools
/// Specifies role-based access requirements for MCP tools
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class McpAuthorizeAttribute : Attribute
{
    /// <summary>
    /// Required roles for accessing this MCP tool
    /// </summary>
    public string[] Roles { get; }

    /// <summary>
    /// Whether the tool allows anonymous access
    /// </summary>
    public bool AllowAnonymous { get; set; } = false;

    /// <summary>
    /// Initialize with required roles
    /// </summary>
    public McpAuthorizeAttribute(params string[] roles)
    {
        Roles = roles ?? Array.Empty<string>();
    }

    /// <summary>
    /// Initialize for anonymous access
    /// </summary>
    public McpAuthorizeAttribute()
    {
        Roles = Array.Empty<string>();
        AllowAnonymous = true;
    }
}

/// <summary>
/// Built-in role constants for MCP authorization
/// </summary>
public static class McpRoles
{
    /// <summary>
    /// Administrator role - full access to all tools
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Sales role - access to sales and customer tools
    /// </summary>
    public const string Sales = "Sales";

    /// <summary>
    /// Customer service role - access to customer service tools
    /// </summary>
    public const string CustomerService = "CustomerService";

    /// <summary>
    /// Read-only access role
    /// </summary>
    public const string ReadOnly = "ReadOnly";

    /// <summary>
    /// All standard business roles
    /// </summary>
    public static readonly string[] AllBusinessRoles = { Admin, Sales, CustomerService };

    /// <summary>
    /// Roles that can access sensitive operations
    /// </summary>
    public static readonly string[] SensitiveOperationRoles = { Admin };

    /// <summary>
    /// Roles that can access customer data
    /// </summary>
    public static readonly string[] CustomerDataRoles = { Admin, Sales, CustomerService };
}
