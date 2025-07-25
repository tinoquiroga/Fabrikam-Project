using FabrikamContracts.DTOs.Orders;

namespace FabrikamContracts.DTOs.Support;

/// <summary>
/// Support ticket list item structure
/// This DTO aligns exactly with SupportTicketsController.GetSupportTickets() API response
/// </summary>
public class SupportTicketListItemDto
{
    /// <summary>
    /// Ticket ID - matches API property 'id'
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ticket number - matches API property 'ticketNumber'
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>
    /// Ticket subject - matches API property 'subject'
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Ticket status - matches API property 'status'
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Ticket priority - matches API property 'priority'
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Ticket category - matches API property 'category'
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Customer information - matches API property 'customer'
    /// </summary>
    public SupportCustomerDto Customer { get; set; } = new();

    /// <summary>
    /// Assigned to - matches API property 'assignedTo'
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Created date - matches API property 'createdDate'
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Resolved date - matches API property 'resolvedDate'
    /// </summary>
    public DateTime? ResolvedDate { get; set; }

    /// <summary>
    /// Region - matches API property 'region'
    /// </summary>
    public string? Region { get; set; }
}

/// <summary>
/// Customer information within support ticket
/// </summary>
public class SupportCustomerDto
{
    /// <summary>
    /// Customer ID - matches API property 'customer.id'
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Customer name - matches API property 'customer.name'
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Customer region - matches API property 'customer.region'
    /// </summary>
    public string? Region { get; set; }
}

/// <summary>
/// Support ticket information
/// </summary>
public class SupportTicketDetailDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public List<TicketNoteDto> Notes { get; set; } = new();
}

/// <summary>
/// Support ticket collection information
/// </summary>
public class SupportTicketInfoDto
{
    public List<SupportTicketDetailDto> Tickets { get; set; } = new();
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ResolvedTickets { get; set; }
}

/// <summary>
/// Ticket note information
/// </summary>
public class TicketNoteDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsInternal { get; set; }
}

/// <summary>
/// Support analytics response structure (for SupportTicketsController.GetSupportAnalytics)
/// </summary>
public class SupportAnalyticsDto
{
    public SupportSummaryDto Summary { get; set; } = new();
    public List<SupportByStatusDto> ByStatus { get; set; } = new();
    public List<SupportByPriorityDto> ByPriority { get; set; } = new();
    public List<SupportByRegionDto> ByRegion { get; set; } = new();
    public List<SupportTrendDto> Trends { get; set; } = new();
}

/// <summary>
/// Support summary statistics
/// </summary>
public class SupportSummaryDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ClosedTickets { get; set; }
    public double AverageResolutionTime { get; set; }
    public SalesPeriodDto Period { get; set; } = new();
}

/// <summary>
/// Support tickets grouped by status
/// </summary>
public class SupportByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Support tickets grouped by priority
/// </summary>
public class SupportByPriorityDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageResolutionTime { get; set; }
}

/// <summary>
/// Support tickets grouped by region
/// </summary>
public class SupportByRegionDto
{
    public string Region { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageResolutionTime { get; set; }
}

/// <summary>
/// Daily support ticket trends
/// </summary>
public class SupportTrendDto
{
    public string Date { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Resolved { get; set; }
}
