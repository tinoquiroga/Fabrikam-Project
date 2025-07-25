namespace FabrikamContracts.DTOs.Orders;

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
