namespace FabrikamContracts.DTOs.Orders;

/// <summary>
/// Order information structure for list endpoints
/// This DTO aligns exactly with OrdersController.GetOrders() API response
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Order ID - matches API property 'id'
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Order number - matches API property 'orderNumber'
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Order status - matches API property 'status'
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Order date - matches API property 'orderDate'
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Order total - matches API property 'total'
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Customer information - matches API property 'customer'
    /// </summary>
    public OrderCustomerDto Customer { get; set; } = new();
}
