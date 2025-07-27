using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikamApi.Data;
using FabrikamApi.Models;
using FabrikamContracts.DTOs.Orders;

namespace FabrikamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly FabrikamIdentityDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(FabrikamIdentityDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetOrders(
        string? status = null,
        string? region = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(o => o.Status == statusEnum);
                }
            }

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(o => o.Customer.Region == region);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    Status = o.Status.ToString(),
                    o.OrderDate,
                    o.Total,
                    Customer = new
                    {
                        o.Customer.Id,
                        Name = $"{o.Customer.FirstName} {o.Customer.LastName}",
                        o.Customer.Region
                    }
                })
                .ToListAsync();

            // Add total count to response headers
            Response.Headers.Append("X-Total-Count", totalCount.ToString());

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetOrder(int id)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            var result = new
            {
                order.Id,
                order.OrderNumber,
                Status = order.Status.ToString(),
                order.OrderDate,
                order.ShippedDate,
                order.DeliveredDate,
                order.Subtotal,
                order.Tax,
                order.Shipping,
                order.Total,
                Customer = new
                {
                    order.Customer.Id,
                    Name = $"{order.Customer.FirstName} {order.Customer.LastName}",
                    order.Customer.Email,
                    order.Customer.Phone,
                    order.Customer.Region
                },
                Items = order.OrderItems.Select(oi => new
                {
                    oi.Id,
                    Product = new
                    {
                        oi.Product.Id,
                        oi.Product.Name,
                        oi.Product.Category
                    },
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.LineTotal,
                    oi.CustomOptions
                })
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get sales analytics and summary data
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<SalesAnalyticsDto>> GetSalesAnalytics(
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            // Default to last 30 days if no dates provided
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                fromDate = DateTime.UtcNow.AddDays(-30);
                toDate = DateTime.UtcNow;
            }
            else if (!fromDate.HasValue && toDate.HasValue)
            {
                fromDate = toDate.Value.AddDays(-30);
            }
            else if (!toDate.HasValue)
            {
                toDate = DateTime.UtcNow;
            }

            var query = _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate);

            var orders = await query.ToListAsync();

            var analytics = new SalesAnalyticsDto
            {
                Summary = new SalesSummaryDto
                {
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Sum(o => o.Total),
                    AverageOrderValue = orders.Any() ? orders.Average(o => o.Total) : 0,
                    Period = new SalesPeriodDto
                    {
                        FromDate = fromDate!.Value.ToString("yyyy-MM-dd"),
                        ToDate = toDate!.Value.ToString("yyyy-MM-dd")
                    }
                },
                ByStatus = orders
                    .GroupBy(o => o.Status)
                    .Select(g => new SalesByStatusDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        Revenue = g.Sum(o => o.Total)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                ByRegion = orders
                    .GroupBy(o => o.Customer.Region)
                    .Select(g => new SalesByRegionDto
                    {
                        Region = g.Key ?? "Unknown",
                        Count = g.Count(),
                        Revenue = g.Sum(o => o.Total)
                    })
                    .OrderByDescending(x => x.Revenue)
                    .ToList(),
                RecentTrends = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new SalesTrendDto
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Orders = g.Count(),
                        Revenue = g.Sum(o => o.Total)
                    })
                    .OrderBy(x => x.Date)
                    .ToList()
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales analytics");
            return StatusCode(500, "Internal server error");
        }
    }
}