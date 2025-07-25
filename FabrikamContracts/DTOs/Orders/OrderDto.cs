namespace FabrikamContracts.DTOs.Orders;

/// <summary>
/// Order information structure (simplified for API alignment)
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}
