using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.DTOs;

/// <summary>
/// Sales analytics response with comprehensive business metrics
/// </summary>
public class SalesAnalyticsDto
{
    /// <summary>
    /// Summary metrics for the requested period
    /// </summary>
    public SalesSummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Revenue and order breakdown by status
    /// </summary>
    public List<SalesByStatusDto> ByStatus { get; set; } = new();
    
    /// <summary>
    /// Revenue and order breakdown by region
    /// </summary>
    public List<SalesByRegionDto> ByRegion { get; set; } = new();
    
    /// <summary>
    /// Daily sales trends over the period - matches API property 'recentTrends'
    /// </summary>
    public List<DailySalesDto> RecentTrends { get; set; } = new();
}

/// <summary>
/// Summary metrics for sales analytics
/// </summary>
public class SalesSummaryDto
{
    /// <summary>
    /// Total number of orders in the period
    /// </summary>
    public int TotalOrders { get; set; }
    
    /// <summary>
    /// Total revenue amount
    /// </summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>
    /// Average order value
    /// </summary>
    public decimal AverageOrderValue { get; set; }
    
    /// <summary>
    /// Date range for the analytics period
    /// </summary>
    public PeriodDto Period { get; set; } = new();
}

/// <summary>
/// Date period information
/// </summary>
public class PeriodDto
{
    /// <summary>
    /// Start date of the period (ISO 8601 format)
    /// </summary>
    [Required]
    public string FromDate { get; set; } = string.Empty;
    
    /// <summary>
    /// End date of the period (ISO 8601 format)
    /// </summary>
    [Required]
    public string ToDate { get; set; } = string.Empty;
}

/// <summary>
/// Sales breakdown by order status
/// </summary>
public class SalesByStatusDto
{
    /// <summary>
    /// Order status (e.g., "Confirmed", "Delivered", "Cancelled")
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of orders with this status
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Total revenue for orders with this status
    /// </summary>
    public decimal Revenue { get; set; }
    
    /// <summary>
    /// Percentage of total orders
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Sales breakdown by region
/// </summary>
public class SalesByRegionDto
{
    /// <summary>
    /// Region name (e.g., "West", "East", "South")
    /// </summary>
    [Required]
    public string Region { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of orders in this region
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Total revenue from this region
    /// </summary>
    public decimal Revenue { get; set; }
    
    /// <summary>
    /// Percentage of total revenue
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Daily sales trend data
/// </summary>
public class DailySalesDto
{
    /// <summary>
    /// Date for the sales data (ISO 8601 format)
    /// </summary>
    [Required]
    public string Date { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of orders on this date
    /// </summary>
    public int OrderCount { get; set; }
    
    /// <summary>
    /// Revenue amount for this date
    /// </summary>
    public decimal Revenue { get; set; }
}
