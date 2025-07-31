namespace FabrikamContracts.DTOs.Customers;

/// <summary>
/// Customer analytics response structure
/// This DTO aligns exactly with CustomersController.GetCustomersAnalytics() API response
/// </summary>
public class CustomerAnalyticsDto
{
    /// <summary>
    /// Total number of customers - serialized as 'totalCustomers' in JSON
    /// </summary>
    public int TotalCustomers { get; set; }

    /// <summary>
    /// Number of active customers (customers with orders) - serialized as 'activeCustomers' in JSON
    /// </summary>
    public int ActiveCustomers { get; set; }

    /// <summary>
    /// Total revenue from all customers - serialized as 'totalRevenue' in JSON
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Average order value across all orders - serialized as 'averageOrderValue' in JSON
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// Regional breakdown of customers - serialized as 'regionalBreakdown' in JSON
    /// </summary>
    public List<CustomerRegionalBreakdownDto> RegionalBreakdown { get; set; } = new();

    /// <summary>
    /// Top customers by revenue - serialized as 'topCustomers' in JSON
    /// </summary>
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

/// <summary>
/// Regional breakdown data for customer analytics
/// </summary>
public class CustomerRegionalBreakdownDto
{
    /// <summary>
    /// Region name - serialized as 'region' in JSON
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Number of customers in this region - serialized as 'customerCount' in JSON
    /// </summary>
    public int CustomerCount { get; set; }

    /// <summary>
    /// Total revenue from this region - serialized as 'revenue' in JSON
    /// </summary>
    public decimal Revenue { get; set; }
}

/// <summary>
/// Top customer data for analytics
/// </summary>
public class TopCustomerDto
{
    /// <summary>
    /// Customer ID - serialized as 'id' in JSON
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Customer name - serialized as 'name' in JSON
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Customer email - serialized as 'email' in JSON
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer region - serialized as 'region' in JSON
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Total amount spent by this customer - serialized as 'totalSpent' in JSON
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// Number of orders placed by this customer - serialized as 'orderCount' in JSON
    /// </summary>
    public int OrderCount { get; set; }
}
