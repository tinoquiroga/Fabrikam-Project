using Microsoft.AspNetCore.Mvc;

namespace FabrikamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly ILogger<InfoController> _logger;

    public InfoController(ILogger<InfoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get API information and available endpoints
    /// </summary>
    [HttpGet]
    public IActionResult GetApiInfo()
    {
        var buildTime = GetBuildTime();
        var apiInfo = new
        {
            Name = "Fabrikam Modular Homes API",
            Version = "1.0.0",
            BuildTime = buildTime,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Description = "API for managing modular home business operations including sales, inventory, and customer service",
            Documentation = "/swagger",
            Endpoints = new
            {
                Customers = "/api/customers",
                Orders = "/api/orders",
                Products = "/api/products",
                SupportTickets = "/api/supporttickets",
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
