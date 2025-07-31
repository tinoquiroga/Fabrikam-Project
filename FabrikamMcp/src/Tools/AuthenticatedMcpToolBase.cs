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
    /// Set GUID context for disabled authentication mode
    /// </summary>
    protected void SetGuidContext(string? userGuid)
    {
        if (!string.IsNullOrWhiteSpace(userGuid))
        {
            _authService.SetUserGuidContext(userGuid);
            _logger.LogDebug("Set GUID context for tool execution: {UserGuid}", userGuid);
        }
    }

    /// <summary>
    /// Validate and process user GUID parameter
    /// </summary>
    protected bool ValidateAndSetGuidContext(string? userGuid, string methodName)
    {
        if (string.IsNullOrWhiteSpace(userGuid))
        {
            _logger.LogWarning("No user GUID provided for {MethodName} in disabled authentication mode", methodName);
            return false;
        }

        if (!Guid.TryParse(userGuid, out var guidValue) || guidValue == Guid.Empty)
        {
            _logger.LogWarning("Invalid GUID format provided for {MethodName}: {UserGuid}", methodName, userGuid);
            return false;
        }

        SetGuidContext(userGuid);
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
    protected async Task AddAuthenticationHeadersAsync(HttpRequestMessage request)
    {
        // Get JWT token if available (async to support service JWT generation)
        var jwtToken = await _authService.GetCurrentJwtTokenAsync();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            _logger.LogDebug("Added JWT authentication header to API request");
        }
        else
        {
            _logger.LogDebug("No JWT token available for API request");
        }
    }

    /// <summary>
    /// Create HTTP headers with authentication if available (legacy sync version)
    /// </summary>
    protected void AddAuthenticationHeaders(HttpRequestMessage request)
    {
        AddAuthenticationHeadersAsync(request).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Create an authenticated HTTP client request with proper headers
    /// </summary>
    protected async Task<HttpResponseMessage> SendAuthenticatedRequest(string url, HttpMethod? method = null)
    {
        method ??= HttpMethod.Get;
        
        using var request = new HttpRequestMessage(method, url);
        await AddAuthenticationHeadersAsync(request);
        
        return await _httpClient.SendAsync(request);
    }
}
