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
        var apiInfo = new
        {
            Name = "Fabrikam Modular Homes API",
            Version = "1.0.0",
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
}
