using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.Models;

public class SupportTicket
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string TicketNumber { get; set; } = string.Empty;
    
    [Required]
    public int CustomerId { get; set; }
    
    public int? OrderId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    
    [Required]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    [Required]
    public TicketCategory Category { get; set; }
    
    [StringLength(100)]
    public string? AssignedTo { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ResolvedDate { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [StringLength(50)]
    public string? Region { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual ICollection<TicketNote> Notes { get; set; } = new List<TicketNote>();
}

public class TicketNote
{
    public int Id { get; set; }
    
    [Required]
    public int TicketId { get; set; }
    
    [Required]
    public string Note { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public bool IsInternal { get; set; } = false;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual SupportTicket Ticket { get; set; } = null!;
}

public enum TicketStatus
{
    Open = 1,
    InProgress = 2,
    PendingCustomer = 3,
    Resolved = 4,
    Closed = 5,
    Cancelled = 6
}

public enum TicketPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TicketCategory
{
    OrderInquiry = 1,
    DeliveryIssue = 2,
    ProductDefect = 3,
    Installation = 4,
    Billing = 5,
    Technical = 6,
    General = 7,
    Complaint = 8
}
