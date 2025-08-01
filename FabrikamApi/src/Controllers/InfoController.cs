using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using FabrikamContracts.DTOs;

namespace FabrikamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly ILogger<InfoController> _logger;
    private readonly IConfiguration _configuration;
    private readonly AuthenticationSettings _authSettings;
    private readonly IWebHostEnvironment _environment;

    public InfoController(ILogger<InfoController> logger, IConfiguration configuration, AuthenticationSettings authSettings, IWebHostEnvironment environment)
    {
        _logger = logger;
        _configuration = configuration;
        _authSettings = authSettings;
        _environment = environment;
    }

    /// <summary>
    /// Get API information and available endpoints
    /// </summary>
    [HttpGet]
    public IActionResult GetApiInfo()
    {
        var buildTime = GetBuildTime();
        var authMode = GetAuthenticationMode();
        
        // Use proper DTO following C# best practices
        var apiInfo = new ApiInfoResponse
        {
            ApplicationName = "Fabrikam Modular Homes API",
            Version = "1.1.0",
            BuildDate = buildTime,
            Environment = _environment.EnvironmentName,
            DatabaseStatus = "Connected"
        };

        return Ok(apiInfo);
    }

    /// <summary>
    /// Get health status of the API
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        var healthInfo = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            services = new
            {
                database = "Connected",
                api = "Running"
            }
        };

        return Ok(healthInfo);
    }

    /// <summary>
    /// Get just the version information for quick comparison
    /// </summary>
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        var buildTime = GetBuildTime();
        var versionInfo = new
        {
            Version = "1.0.0",
            BuildTime = buildTime,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            ServerTime = DateTime.UtcNow
        };

        return Ok(versionInfo);
    }

    /// <summary>
    /// Get authentication configuration information
    /// </summary>
    [HttpGet("auth")]
    public IActionResult GetAuthInfo()
    {
        var authMode = GetAuthenticationMode();
        var authInfo = new
        {
            Mode = authMode,
            Description = GetAuthenticationDescription(authMode),
            Endpoints = GetAuthenticationEndpoints(authMode),
            IsEnabled = authMode != "Disabled",
            RequiresToken = authMode == "JwtTokens" || authMode == "EntraExternalId"
        };

        return Ok(authInfo);
    }

    private string GetAuthenticationMode()
    {
        // Use the configured authentication settings to ensure consistency
        return _authSettings.Mode.ToString();
    }

    private string GetAuthenticationDescription(string mode)
    {
        return mode switch
        {
            "Disabled" => "Authentication is disabled. All endpoints are publicly accessible.",
            "BearerToken" => "JWT token-based authentication using ASP.NET Core Identity. Tokens expire and require refresh.",
            "JwtTokens" => "JWT token-based authentication using ASP.NET Core Identity. Tokens expire and require refresh.",
            "EntraExternalId" => "Azure Entra External ID authentication for external users and applications.",
            _ => "Unknown authentication mode"
        };
    }

    private object? GetAuthenticationEndpoints(string mode)
    {
        return mode switch
        {
            "Disabled" => null,
            "BearerToken" => new
            {
                Register = "/api/auth/register",
                Login = "/api/auth/login", 
                Refresh = "/api/auth/refresh",
                DemoCredentials = "/api/auth/demo-credentials"
            },
            "JwtTokens" => new
            {
                Register = "/api/auth/register",
                Login = "/api/auth/login", 
                Refresh = "/api/auth/refresh",
                DemoCredentials = "/api/auth/demo-credentials"
            },
            "EntraExternalId" => new
            {
                Login = "/api/auth/entra/login",
                Callback = "/api/auth/entra/callback"
            },
            _ => null
        };
    }

    private string GetBuildTime()
    {
        try
        {
            // Get the build time from the assembly
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fileInfo = new FileInfo(assembly.Location);
            return fileInfo.LastWriteTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"); // ISO 8601 format for better parsing
        }
        catch
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); // Fallback to current time in ISO format
        }
    }
}
