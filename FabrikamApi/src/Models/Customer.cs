using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.Models;

public class Customer
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(200)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(10)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? ZipCode { get; set; }
    
    [StringLength(50)]
    public string? Region { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
