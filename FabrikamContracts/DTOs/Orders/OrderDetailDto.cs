namespace FabrikamContracts.DTOs.Orders;

/// <summary>
/// Order detailed information (for GetOrder endpoint)
/// </summary>
public class OrderDetailDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public OrderCustomerDto Customer { get; set; } = new();
    public List<OrderItemDetailDto> Items { get; set; } = new();
}

/// <summary>
/// Order customer information
/// </summary>
public class OrderCustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}

/// <summary>
/// Order item detailed information
/// </summary>
public class OrderItemDetailDto
{
    public int Id { get; set; }
    public OrderItemProductDto Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? CustomOptions { get; set; }
}

/// <summary>
/// Order item product information
/// </summary>
public class OrderItemProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
