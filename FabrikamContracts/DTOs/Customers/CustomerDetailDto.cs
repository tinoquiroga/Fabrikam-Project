namespace FabrikamContracts.DTOs.Customers;

/// <summary>
/// Customer detailed information (for GetCustomer endpoint)
/// </summary>
public class CustomerDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public CustomerAddressDto Address { get; set; } = new();
    public string Region { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public CustomerOrderSummaryDto OrderSummary { get; set; } = new();
    public List<CustomerRecentOrderDto> RecentOrders { get; set; } = new();
    public CustomerSupportSummaryDto SupportTicketSummary { get; set; } = new();
}

/// <summary>
/// Customer address information
/// </summary>
public class CustomerAddressDto
{
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

/// <summary>
/// Customer order summary
/// </summary>
public class CustomerOrderSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

/// <summary>
/// Customer recent order information
/// </summary>
public class CustomerRecentOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Customer support ticket summary
/// </summary>
public class CustomerSupportSummaryDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public List<CustomerRecentTicketDto> RecentTickets { get; set; } = new();
}

/// <summary>
/// Customer recent ticket information
/// </summary>
public class CustomerRecentTicketDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
