using System.ComponentModel.DataAnnotations;

namespace FabrikamContracts.DTOs;

/// <summary>
/// Product information structure
/// This DTO aligns exactly with ProductsController.GetProducts() API response
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Product ID - matches API property 'id'
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Product name - matches API property 'name'
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Model number - matches API property 'modelNumber'
    /// </summary>
    public string ModelNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category - matches API property 'category'
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Product price - matches API property 'price'
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Stock quantity - matches API property 'stockQuantity'
    /// </summary>
    public int StockQuantity { get; set; }
    
    /// <summary>
    /// Reorder level - matches API property 'reorderLevel'
    /// </summary>
    public int ReorderLevel { get; set; }
    
    /// <summary>
    /// Stock status - matches API property 'stockStatus'
    /// </summary>
    public string StockStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Dimensions - matches API property 'dimensions'
    /// </summary>
    public string? Dimensions { get; set; }
    
    /// <summary>
    /// Square feet - matches API property 'squareFeet'
    /// </summary>
    public int? SquareFeet { get; set; }
    
    /// <summary>
    /// Number of bedrooms - matches API property 'bedrooms'
    /// </summary>
    public int? Bedrooms { get; set; }
    
    /// <summary>
    /// Number of bathrooms - matches API property 'bathrooms'
    /// </summary>
    public int? Bathrooms { get; set; }
    
    /// <summary>
    /// Delivery estimate in days - matches API property 'deliveryDaysEstimate'
    /// </summary>
    public int DeliveryDaysEstimate { get; set; }
}

/// <summary>
/// Customer list item structure
/// This DTO aligns exactly with CustomersController.GetCustomers() API response
/// </summary>
public class CustomerListItemDto
{
    /// <summary>
    /// Customer ID - matches API property 'id'
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Full customer name - matches API property 'name'
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address - matches API property 'email'
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number - matches API property 'phone'
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// City - matches API property 'city'
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// State - matches API property 'state'
    /// </summary>
    public string State { get; set; } = string.Empty;
    
    /// <summary>
    /// Region - matches API property 'region'
    /// </summary>
    public string Region { get; set; } = string.Empty;
    
    /// <summary>
    /// Registration date - matches API property 'createdDate'
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Number of orders - matches API property 'orderCount'
    /// </summary>
    public int OrderCount { get; set; }
    
    /// <summary>
    /// Total amount spent - matches API property 'totalSpent'
    /// </summary>
    public decimal TotalSpent { get; set; }
}

/// <summary>
/// Sales analytics response structure
/// This DTO aligns exactly with OrdersController.GetSalesAnalytics() API response
/// </summary>
public class SalesAnalyticsDto
{
    /// <summary>
    /// Summary statistics - matches API property 'summary'
    /// </summary>
    public SalesSummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Status breakdown - matches API property 'byStatus'
    /// </summary>
    public List<SalesByStatusDto> ByStatus { get; set; } = new();
    
    /// <summary>
    /// Region breakdown - matches API property 'byRegion'
    /// </summary>
    public List<SalesByRegionDto> ByRegion { get; set; } = new();
    
    /// <summary>
    /// Daily trends - matches API property 'recentTrends'
    /// </summary>
    public List<SalesTrendDto> RecentTrends { get; set; } = new();
}

/// <summary>
/// Sales summary statistics
/// </summary>
public class SalesSummaryDto
{
    /// <summary>
    /// Total number of orders in the period
    /// </summary>
    public int TotalOrders { get; set; }
    
    /// <summary>
    /// Total revenue for the period
    /// </summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>
    /// Average order value
    /// </summary>
    public decimal AverageOrderValue { get; set; }
    
    /// <summary>
    /// Period information
    /// </summary>
    public SalesPeriodDto Period { get; set; } = new();
}

/// <summary>
/// Sales period definition
/// </summary>
public class SalesPeriodDto
{
    /// <summary>
    /// Start date of the period (YYYY-MM-DD format)
    /// </summary>
    public string FromDate { get; set; } = string.Empty;
    
    /// <summary>
    /// End date of the period (YYYY-MM-DD format)
    /// </summary>
    public string ToDate { get; set; } = string.Empty;
}

/// <summary>
/// Sales data grouped by order status
/// </summary>
public class SalesByStatusDto
{
    /// <summary>
    /// Order status name
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of orders with this status
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Total revenue from orders with this status
    /// </summary>
    public decimal Revenue { get; set; }
}

/// <summary>
/// Sales data grouped by customer region
/// </summary>
public class SalesByRegionDto
{
    /// <summary>
    /// Region name
    /// </summary>
    public string Region { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of orders from this region
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Total revenue from this region
    /// </summary>
    public decimal Revenue { get; set; }
}

/// <summary>
/// Daily sales trend data
/// </summary>
public class SalesTrendDto
{
    /// <summary>
    /// Date in YYYY-MM-DD format
    /// </summary>
    public string Date { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of orders on this date
    /// </summary>
    public int Orders { get; set; }
    
    /// <summary>
    /// Total revenue on this date
    /// </summary>
    public decimal Revenue { get; set; }
}

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

/// <summary>
/// Inventory status information
/// </summary>
public class InventoryStatusDto
{
    public List<ProductDto> LowStockProducts { get; set; } = new();
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
}

/// <summary>
/// Product inventory information
/// </summary>
public class ProductInventoryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public bool NeedsReorder { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
}

/// <summary>
/// Detailed product information
/// </summary>
public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string StockStatus { get; set; } = string.Empty;
    public string? Dimensions { get; set; }
    public int? SquareFeet { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int DeliveryDaysEstimate { get; set; }
    public string Description { get; set; } = string.Empty;
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
