using System.ComponentModel;
using System.Reflection;
using FabrikamMcp.Attributes;
using FabrikamMcp.Services;

namespace FabrikamMcp.Tools;

/// <summary>
/// Base class for authenticated MCP tools
/// Provides authentication context and authorization validation
/// </summary>
public abstract class AuthenticatedMcpToolBase
{
    protected readonly HttpClient _httpClient;
    protected readonly IConfiguration _configuration;
    protected readonly IAuthenticationService _authService;
    protected readonly ILogger _logger;

    protected AuthenticatedMcpToolBase(
        HttpClient httpClient, 
        IConfiguration configuration, 
        IAuthenticationService authService,
        ILogger logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Validate authorization for the current method call
    /// </summary>
    protected virtual bool ValidateAuthorization(string methodName)
    {
        // Get the method info
        var method = GetType().GetMethod(methodName);
        if (method == null)
        {
            _logger.LogWarning("Method {MethodName} not found for authorization check", methodName);
            return false;
        }

        // Check for McpAuthorize attribute
        var authorizeAttribute = method.GetCustomAttribute<McpAuthorizeAttribute>();
        if (authorizeAttribute == null)
        {
            // No authorization attribute - require authentication by default
            return _authService.IsAuthenticated();
        }

        // Allow anonymous access if specified
        if (authorizeAttribute.AllowAnonymous)
        {
            return true;
        }

        // Check authentication
        if (!_authService.IsAuthenticated())
        {
            _logger.LogWarning("Unauthorized access attempt to {MethodName} - user not authenticated", methodName);
            return false;
        }

        // Check role requirements
        if (authorizeAttribute.Roles.Any())
        {
            var hasRequiredRole = authorizeAttribute.Roles.Any(role => _authService.HasRole(role));
            if (!hasRequiredRole)
            {
                var userRoles = string.Join(", ", _authService.GetCurrentUserRoles());
                var requiredRoles = string.Join(", ", authorizeAttribute.Roles);
                _logger.LogWarning("Unauthorized access attempt to {MethodName} - user has roles [{UserRoles}] but requires [{RequiredRoles}]", 
                    methodName, userRoles, requiredRoles);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Create an authentication-aware error response
    /// </summary>
    protected object CreateAuthenticationErrorResponse(string methodName)
    {
        var context = _authService.CreateAuthenticationContext();
        
        string errorMessage;
        if (!context.IsAuthenticated)
        {
            errorMessage = "Authentication required. Please provide a valid JWT bearer token.";
        }
        else
        {
            errorMessage = $"Insufficient permissions. Your roles: [{string.Join(", ", context.Roles)}]";
        }

        _logger.LogWarning("Authentication error for {MethodName}: {ErrorMessage} (User: {User})", 
            methodName, errorMessage, context.GetDisplayName());

        return new
        {
            error = new
            {
                code = context.IsAuthenticated ? 403 : 401,
                message = errorMessage,
                method = methodName,
                timestamp = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Log tool usage for auditing
    /// </summary>
    protected void LogToolUsage(string methodName, object? parameters = null)
    {
        var context = _authService.CreateAuthenticationContext();
        
        _logger.LogInformation("MCP Tool Usage: {MethodName} by {User} with roles [{Roles}]", 
            methodName, context.GetDisplayName(), string.Join(", ", context.Roles));

        if (parameters != null)
        {
            _logger.LogDebug("MCP Tool Parameters for {MethodName}: {@Parameters}", methodName, parameters);
        }
    }

    /// <summary>
    /// Get the API base URL from configuration
    /// </summary>
    protected string GetApiBaseUrl()
    {
        return _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
    }

    /// <summary>
    /// Create HTTP headers with authentication if available
    /// </summary>
    protected void AddAuthenticationHeaders(HttpRequestMessage request)
    {
        // Note: For API calls, we might want to forward the JWT token
        // This would be implemented when we add API-to-API authentication
        // For now, MCP authentication is separate from API authentication
    }
}
