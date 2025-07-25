namespace FabrikamContracts.DTOs.Customers;

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
