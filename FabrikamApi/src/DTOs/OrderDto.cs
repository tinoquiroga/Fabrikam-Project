using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.DTOs;

/// <summary>
/// Comprehensive order information with details and items
/// </summary>
public class OrderListDto
{
    /// <summary>
    /// Order summary information
    /// </summary>
    public OrderSummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Orders organized by status
    /// </summary>
    public List<OrdersByStatusDto> ByStatus { get; set; } = new();
    
    /// <summary>
    /// Recent orders with full details
    /// </summary>
    public List<OrderDetailDto> RecentOrders { get; set; } = new();
}

/// <summary>
/// High-level order summary metrics
/// </summary>
public class OrderSummaryDto
{
    /// <summary>
    /// Total number of orders in the result set
    /// </summary>
    public int TotalOrders { get; set; }
    
    /// <summary>
    /// Total revenue from all orders
    /// </summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>
    /// Average order value
    /// </summary>
    public decimal AverageOrderValue { get; set; }
    
    /// <summary>
    /// Date range of the orders
    /// </summary>
    public DateRangeDto DateRange { get; set; } = new();
    
    /// <summary>
    /// Regional breakdown if applicable
    /// </summary>
    public List<RegionalOrderSummaryDto> RegionalBreakdown { get; set; } = new();
}

/// <summary>
/// Orders grouped by status
/// </summary>
public class OrdersByStatusDto
{
    /// <summary>
    /// Order status name
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of orders in this status
    /// </summary>
    public int OrderCount { get; set; }
    
    /// <summary>
    /// Total revenue for orders in this status
    /// </summary>
    public decimal Revenue { get; set; }
    
    /// <summary>
    /// Orders in this status
    /// </summary>
    public List<OrderDetailDto> Orders { get; set; } = new();
}

/// <summary>
/// Detailed order information
/// </summary>
public class OrderDetailDto
{
    /// <summary>
    /// Unique order identifier
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Human-readable order number
    /// </summary>
    [Required]
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer who placed the order
    /// </summary>
    public OrderCustomerDto Customer { get; set; } = new();
    
    /// <summary>
    /// Current order status
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// When the order was placed
    /// </summary>
    public DateTime OrderDate { get; set; }
    
    /// <summary>
    /// When the order was shipped (if applicable)
    /// </summary>
    public DateTime? ShippedDate { get; set; }
    
    /// <summary>
    /// When the order was delivered (if applicable)
    /// </summary>
    public DateTime? DeliveredDate { get; set; }
    
    /// <summary>
    /// Order financial information
    /// </summary>
    public OrderFinancialDto Financials { get; set; } = new();
    
    /// <summary>
    /// Shipping address information
    /// </summary>
    public OrderShippingDto Shipping { get; set; } = new();
    
    /// <summary>
    /// Items included in the order
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new();
    
    /// <summary>
    /// Order notes or special instructions
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Geographic region for the order
    /// </summary>
    public string? Region { get; set; }
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Customer information for order context
/// </summary>
public class OrderCustomerDto
{
    /// <summary>
    /// Customer ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Customer name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer email
    /// </summary>
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer phone number
    /// </summary>
    public string? Phone { get; set; }
}

/// <summary>
/// Order financial breakdown
/// </summary>
public class OrderFinancialDto
{
    /// <summary>
    /// Subtotal before tax and shipping
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Tax amount
    /// </summary>
    public decimal Tax { get; set; }
    
    /// <summary>
    /// Shipping cost
    /// </summary>
    public decimal Shipping { get; set; }
    
    /// <summary>
    /// Total order amount
    /// </summary>
    public decimal Total { get; set; }
}

/// <summary>
/// Order shipping information
/// </summary>
public class OrderShippingDto
{
    /// <summary>
    /// Shipping street address
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Shipping city
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Shipping state/province
    /// </summary>
    public string? State { get; set; }
    
    /// <summary>
    /// Shipping postal/zip code
    /// </summary>
    public string? ZipCode { get; set; }
    
    /// <summary>
    /// Full formatted shipping address
    /// </summary>
    public string FormattedAddress => 
        string.Join(", ", new[] { Address, City, State, ZipCode }.Where(s => !string.IsNullOrEmpty(s)));
}

/// <summary>
/// Individual order item details
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// Order item ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Product information
    /// </summary>
    public OrderProductDto Product { get; set; } = new();
    
    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Unit price at time of order
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Total line amount (quantity × unit price)
    /// </summary>
    public decimal LineTotal { get; set; }
    
    /// <summary>
    /// Custom options or configurations
    /// </summary>
    public string? CustomOptions { get; set; }
}

/// <summary>
/// Product information in order context
/// </summary>
public class OrderProductDto
{
    /// <summary>
    /// Product ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Product name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Current product price (may differ from order price)
    /// </summary>
    public decimal CurrentPrice { get; set; }
}

/// <summary>
/// Regional order summary
/// </summary>
public class RegionalOrderSummaryDto
{
    /// <summary>
    /// Region name
    /// </summary>
    [Required]
    public string Region { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of orders in this region
    /// </summary>
    public int OrderCount { get; set; }
    
    /// <summary>
    /// Total revenue from this region
    /// </summary>
    public decimal Revenue { get; set; }
    
    /// <summary>
    /// Percentage of total orders
    /// </summary>
    public decimal PercentageOfTotal { get; set; }
}

/// <summary>
/// Date range information
/// </summary>
public class DateRangeDto
{
    /// <summary>
    /// Start date of the range
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End date of the range
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Human-readable description of the range
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
