using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FabrikamApi.Data;
using FabrikamApi.Models;
using FabrikamContracts.DTOs.Support;
using Microsoft.AspNetCore.Authorization;

namespace FabrikamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiAccess")] // SECURITY: Environment-aware authentication for all support ticket endpoints
public class SupportTicketsController : ControllerBase
{
    private readonly FabrikamIdentityDbContext _context;
    private readonly ILogger<SupportTicketsController> _logger;

    public SupportTicketsController(FabrikamIdentityDbContext context, ILogger<SupportTicketsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all support tickets with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupportTicketListItemDto>>> GetSupportTickets(
        string? status = null,
        string? priority = null,
        string? category = null,
        string? region = null,
        string? assignedTo = null,
        bool urgent = false,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var query = _context.SupportTickets
                .Include(t => t.Customer)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                // Handle multiple statuses separated by comma
                var statuses = status.Split(',');
                var statusEnums = new List<TicketStatus>();

                foreach (var s in statuses)
                {
                    if (Enum.TryParse<TicketStatus>(s.Trim(), true, out var statusEnum))
                    {
                        statusEnums.Add(statusEnum);
                    }
                }

                if (statusEnums.Any())
                {
                    query = query.Where(t => statusEnums.Contains(t.Status));
                }
                else
                {
                    // If no valid statuses were parsed, return empty result
                    return Ok(new List<SupportTicketListItemDto>());
                }
            }

            if (!string.IsNullOrEmpty(priority))
            {
                if (Enum.TryParse<TicketPriority>(priority, true, out var priorityEnum))
                {
                    query = query.Where(t => t.Priority == priorityEnum);
                }
            }

            if (!string.IsNullOrEmpty(category))
            {
                if (Enum.TryParse<TicketCategory>(category, true, out var categoryEnum))
                {
                    query = query.Where(t => t.Category == categoryEnum);
                }
            }

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(t => t.Region == region);
            }

            if (!string.IsNullOrEmpty(assignedTo))
            {
                query = query.Where(t => t.AssignedTo == assignedTo);
            }

            if (urgent)
            {
                query = query.Where(t => t.Priority == TicketPriority.High || t.Priority == TicketPriority.Critical);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var tickets = await query
                .OrderByDescending(t => t.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new SupportTicketListItemDto
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Subject = t.Subject,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    Category = t.Category.ToString(),
                    Customer = new SupportCustomerDto
                    {
                        Id = t.Customer.Id,
                        Name = $"{t.Customer.FirstName} {t.Customer.LastName}",
                        Region = t.Customer.Region
                    },
                    AssignedTo = t.AssignedTo,
                    CreatedDate = t.CreatedDate,
                    ResolvedDate = t.ResolvedDate,
                    Region = t.Region
                })
                .ToListAsync();

            // Add total count to response headers
            Response.Headers.Append("X-Total-Count", totalCount.ToString());

            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support tickets");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific support ticket by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSupportTicket(int id)
    {
        try
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.Customer)
                .Include(t => t.Order)
                .Include(t => t.Notes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound($"Support ticket with ID {id} not found");
            }

            var result = new
            {
                ticket.Id,
                ticket.TicketNumber,
                ticket.Subject,
                ticket.Description,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority.ToString(),
                Category = ticket.Category.ToString(),
                Customer = new
                {
                    ticket.Customer.Id,
                    Name = $"{ticket.Customer.FirstName} {ticket.Customer.LastName}",
                    ticket.Customer.Email,
                    ticket.Customer.Phone,
                    ticket.Customer.Region
                },
                RelatedOrder = ticket.Order != null ? new
                {
                    ticket.Order.Id,
                    ticket.Order.OrderNumber,
                    Status = ticket.Order.Status.ToString()
                } : null,
                ticket.AssignedTo,
                ticket.CreatedDate,
                ticket.ResolvedDate,
                ticket.LastUpdated,
                ticket.Region,
                Notes = ticket.Notes
                    .OrderByDescending(n => n.CreatedDate)
                    .Select(n => new
                    {
                        n.Id,
                        n.Note,
                        n.CreatedBy,
                        n.CreatedDate,
                        n.IsInternal
                    })
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support ticket {TicketId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get support ticket analytics
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<object>> GetSupportAnalytics(
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

            var query = _context.SupportTickets
                .Include(t => t.Customer)
                .Where(t => t.CreatedDate >= fromDate && t.CreatedDate <= toDate);

            var tickets = await query.ToListAsync();

            var analytics = new
            {
                Summary = new
                {
                    TotalTickets = tickets.Count,
                    OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
                    InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
                    ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                    ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
                    AverageResolutionDays = tickets
                        .Where(t => t.ResolvedDate.HasValue)
                        .Select(t => (t.ResolvedDate!.Value - t.CreatedDate).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average(),
                    Period = new
                    {
                        FromDate = fromDate!.Value.ToString("yyyy-MM-dd"),
                        ToDate = toDate!.Value.ToString("yyyy-MM-dd")
                    }
                },
                ByStatus = tickets
                    .GroupBy(t => t.Status)
                    .Select(g => new
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / tickets.Count * 100, 1)
                    })
                    .OrderByDescending(x => x.Count),
                ByPriority = tickets
                    .GroupBy(t => t.Priority)
                    .Select(g => new
                    {
                        Priority = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / tickets.Count * 100, 1)
                    })
                    .OrderBy(x => x.Priority),
                ByCategory = tickets
                    .GroupBy(t => t.Category)
                    .Select(g => new
                    {
                        Category = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / tickets.Count * 100, 1)
                    })
                    .OrderByDescending(x => x.Count),
                ByRegion = tickets
                    .GroupBy(t => t.Customer.Region)
                    .Select(g => new
                    {
                        Region = g.Key ?? "Unknown",
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / tickets.Count * 100, 1)
                    })
                    .OrderByDescending(x => x.Count)
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support analytics");
            return StatusCode(500, "Internal server error");
        }
    }
}