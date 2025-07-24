using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.DTOs;

/// <summary>
/// Comprehensive support ticket information and analytics
/// </summary>
public class SupportTicketInfoDto
{
    /// <summary>
    /// Support ticket summary statistics
    /// </summary>
    public SupportSummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Individual support ticket records
    /// </summary>
    public List<SupportTicketDetailDto> Tickets { get; set; } = new();
    
    /// <summary>
    /// Support metrics and performance data
    /// </summary>
    public SupportMetricsDto Metrics { get; set; } = new();
}

/// <summary>
/// Overall support system summary
/// </summary>
public class SupportSummaryDto
{
    /// <summary>
    /// Total number of tickets in system
    /// </summary>
    public int TotalTickets { get; set; }
    
    /// <summary>
    /// Number of open/active tickets
    /// </summary>
    public int OpenTickets { get; set; }
    
    /// <summary>
    /// Number of tickets in progress
    /// </summary>
    public int InProgressTickets { get; set; }
    
    /// <summary>
    /// Number of resolved tickets
    /// </summary>
    public int ResolvedTickets { get; set; }
    
    /// <summary>
    /// Number of closed tickets
    /// </summary>
    public int ClosedTickets { get; set; }
    
    /// <summary>
    /// Tickets by priority level
    /// </summary>
    public List<TicketsByPriorityDto> ByPriority { get; set; } = new();
    
    /// <summary>
    /// Tickets by category/type
    /// </summary>
    public List<TicketsByCategoryDto> ByCategory { get; set; } = new();
}

/// <summary>
/// Detailed support ticket information
/// </summary>
public class SupportTicketDetailDto
{
    /// <summary>
    /// Unique ticket identifier
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Ticket title/subject
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the ticket
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty; // Open, In Progress, Resolved, Closed
    
    /// <summary>
    /// Priority level
    /// </summary>
    [Required]
    public string Priority { get; set; } = string.Empty; // Low, Medium, High, Critical
    
    /// <summary>
    /// Category or type of issue
    /// </summary>
    [Required]
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Date ticket was created
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Date ticket was last updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }
    
    /// <summary>
    /// Date ticket was resolved (if applicable)
    /// </summary>
    public DateTime? ResolvedDate { get; set; }
    
    /// <summary>
    /// Customer who submitted the ticket
    /// </summary>
    public TicketCustomerDto Customer { get; set; } = new();
    
    /// <summary>
    /// Assigned support agent (if any)
    /// </summary>
    public SupportAgentDto? AssignedAgent { get; set; }
    
    /// <summary>
    /// Resolution details (if resolved)
    /// </summary>
    public TicketResolutionDto? Resolution { get; set; }
    
    /// <summary>
    /// Related product or order information
    /// </summary>
    public TicketRelatedInfoDto RelatedInfo { get; set; } = new();
}

/// <summary>
/// Customer information for support ticket
/// </summary>
public class TicketCustomerDto
{
    /// <summary>
    /// Customer ID
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Customer name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer email
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// Support agent information
/// </summary>
public class SupportAgentDto
{
    /// <summary>
    /// Agent ID
    /// </summary>
    public int AgentId { get; set; }
    
    /// <summary>
    /// Agent name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Agent email
    /// </summary>
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Agent department or team
    /// </summary>
    public string Department { get; set; } = string.Empty;
}

/// <summary>
/// Ticket resolution information
/// </summary>
public class TicketResolutionDto
{
    /// <summary>
    /// Resolution description/notes
    /// </summary>
    [Required]
    public string Resolution { get; set; } = string.Empty;
    
    /// <summary>
    /// Date resolved
    /// </summary>
    public DateTime ResolvedDate { get; set; }
    
    /// <summary>
    /// Agent who resolved the ticket
    /// </summary>
    public SupportAgentDto ResolvedBy { get; set; } = new();
    
    /// <summary>
    /// Customer satisfaction rating (if provided)
    /// </summary>
    public int? SatisfactionRating { get; set; } // 1-5 scale
    
    /// <summary>
    /// Customer feedback on resolution
    /// </summary>
    public string CustomerFeedback { get; set; } = string.Empty;
}

/// <summary>
/// Related product/order information for ticket
/// </summary>
public class TicketRelatedInfoDto
{
    /// <summary>
    /// Related order ID (if applicable)
    /// </summary>
    public int? OrderId { get; set; }
    
    /// <summary>
    /// Related product ID (if applicable)
    /// </summary>
    public int? ProductId { get; set; }
    
    /// <summary>
    /// Related product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional context or reference information
    /// </summary>
    public string AdditionalContext { get; set; } = string.Empty;
}

/// <summary>
/// Support system performance metrics
/// </summary>
public class SupportMetricsDto
{
    /// <summary>
    /// Average time to first response (in hours)
    /// </summary>
    public decimal AverageResponseTime { get; set; }
    
    /// <summary>
    /// Average time to resolution (in hours)
    /// </summary>
    public decimal AverageResolutionTime { get; set; }
    
    /// <summary>
    /// Customer satisfaction average rating
    /// </summary>
    public decimal AverageSatisfactionRating { get; set; }
    
    /// <summary>
    /// First contact resolution rate (percentage)
    /// </summary>
    public decimal FirstContactResolutionRate { get; set; }
    
    /// <summary>
    /// Ticket volume trend (daily averages)
    /// </summary>
    public List<DailyTicketVolumeDto> VolumeeTrend { get; set; } = new();
}

/// <summary>
/// Tickets grouped by priority level
/// </summary>
public class TicketsByPriorityDto
{
    /// <summary>
    /// Priority level name
    /// </summary>
    [Required]
    public string Priority { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of tickets at this priority
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Percentage of total tickets
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Tickets grouped by category
/// </summary>
public class TicketsByCategoryDto
{
    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of tickets in this category
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Percentage of total tickets
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Daily ticket volume data
/// </summary>
public class DailyTicketVolumeDto
{
    /// <summary>
    /// Date
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Number of tickets created on this date
    /// </summary>
    public int TicketsCreated { get; set; }
    
    /// <summary>
    /// Number of tickets resolved on this date
    /// </summary>
    public int TicketsResolved { get; set; }
}
