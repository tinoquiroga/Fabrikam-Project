using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FabrikamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly ILogger<InfoController> _logger;
    private readonly IConfiguration _configuration;

    public InfoController(ILogger<InfoController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get API information and available endpoints
    /// </summary>
    [HttpGet]
    public IActionResult GetApiInfo()
    {
        var buildTime = GetBuildTime();
        var authMode = GetAuthenticationMode();
        
        var apiInfo = new
        {
            Name = "Fabrikam Modular Homes API",
            Version = "1.0.0",
            BuildTime = buildTime,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Description = "API for managing modular home business operations including sales, inventory, and customer service",
            Documentation = "/swagger",
            Authentication = new
            {
                Mode = authMode,
                Description = GetAuthenticationDescription(authMode),
                Endpoints = GetAuthenticationEndpoints(authMode)
            },
            Endpoints = new
            {
                Customers = "/api/customers",
                Orders = "/api/orders", 
                Products = "/api/products",
                SupportTickets = "/api/supporttickets",
                Authentication = authMode != "Disabled" ? "/api/auth" : null,
                Health = "/health"
            },
            BusinessModules = new[]
            {
                "Sales - Manage customer orders and track sales performance",
                "Inventory - Monitor product stock levels and availability", 
                "Customer Service - Handle support tickets and customer inquiries"
            }
        };

        return Ok(apiInfo);
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
        // Check for explicit authentication mode configuration
        var authMode = _configuration["Authentication:Mode"] ?? 
                      Environment.GetEnvironmentVariable("FABRIKAM_AUTH_MODE");
        
        if (!string.IsNullOrEmpty(authMode))
        {
            return authMode;
        }

        // Auto-detect based on configuration
        var jwtSecretKey = _configuration["Authentication:AspNetIdentity:Jwt:SecretKey"];
        var entraSettings = _configuration["Authentication:EntraExternalId:ClientId"];
        
        if (!string.IsNullOrEmpty(entraSettings))
        {
            return "EntraExternalId";
        }
        else if (!string.IsNullOrEmpty(jwtSecretKey))
        {
            return "JwtTokens";
        }
        else
        {
            return "Disabled";
        }
    }

    private string GetAuthenticationDescription(string mode)
    {
        return mode switch
        {
            "Disabled" => "Authentication is disabled. All endpoints are publicly accessible.",
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
            return fileInfo.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss UTC");
        }
        catch
        {
            return "Unknown";
        }
    }
}
