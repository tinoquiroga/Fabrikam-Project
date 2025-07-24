using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.DTOs;

/// <summary>
/// Comprehensive customer information and analytics
/// </summary>
public class CustomerInfoDto
{
    /// <summary>
    /// Customer summary statistics
    /// </summary>
    public CustomerSummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Individual customer records
    /// </summary>
    public List<CustomerDetailDto> Customers { get; set; } = new();
    
    /// <summary>
    /// Customer segmentation analysis
    /// </summary>
    public List<CustomerSegmentDto> Segments { get; set; } = new();
}

/// <summary>
/// Overall customer base summary
/// </summary>
public class CustomerSummaryDto
{
    /// <summary>
    /// Total number of customers
    /// </summary>
    public int TotalCustomers { get; set; }
    
    /// <summary>
    /// Number of active customers (recent purchases)
    /// </summary>
    public int ActiveCustomers { get; set; }
    
    /// <summary>
    /// Number of new customers this period
    /// </summary>
    public int NewCustomers { get; set; }
    
    /// <summary>
    /// Customer retention rate percentage
    /// </summary>
    public decimal RetentionRate { get; set; }
    
    /// <summary>
    /// Average customer lifetime value
    /// </summary>
    public decimal AverageLifetimeValue { get; set; }
    
    /// <summary>
    /// Geographic distribution of customers
    /// </summary>
    public List<CustomerByRegionDto> RegionalDistribution { get; set; } = new();
}

/// <summary>
/// Detailed customer information
/// </summary>
public class CustomerDetailDto
{
    /// <summary>
    /// Unique customer identifier
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Customer first name
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer last name
    /// </summary>
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer mailing address
    /// </summary>
    public CustomerAddressDto Address { get; set; } = new();
    
    /// <summary>
    /// Date customer was registered
    /// </summary>
    public DateTime RegistrationDate { get; set; }
    
    /// <summary>
    /// Date of last purchase or interaction
    /// </summary>
    public DateTime? LastActivity { get; set; }
    
    /// <summary>
    /// Customer status (Active, Inactive, VIP, etc.)
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer purchase history summary
    /// </summary>
    public CustomerPurchaseHistoryDto PurchaseHistory { get; set; } = new();
    
    /// <summary>
    /// Customer preferences and notes
    /// </summary>
    public CustomerPreferencesDto Preferences { get; set; } = new();
}

/// <summary>
/// Customer address information
/// </summary>
public class CustomerAddressDto
{
    /// <summary>
    /// Street address
    /// </summary>
    public string Street { get; set; } = string.Empty;
    
    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// State or province
    /// </summary>
    public string State { get; set; } = string.Empty;
    
    /// <summary>
    /// Postal or ZIP code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Customer purchase history summary
/// </summary>
public class CustomerPurchaseHistoryDto
{
    /// <summary>
    /// Total number of orders placed
    /// </summary>
    public int TotalOrders { get; set; }
    
    /// <summary>
    /// Total amount spent (lifetime value)
    /// </summary>
    public decimal TotalSpent { get; set; }
    
    /// <summary>
    /// Average order value
    /// </summary>
    public decimal AverageOrderValue { get; set; }
    
    /// <summary>
    /// Date of first purchase
    /// </summary>
    public DateTime? FirstPurchase { get; set; }
    
    /// <summary>
    /// Date of most recent purchase
    /// </summary>
    public DateTime? LastPurchase { get; set; }
    
    /// <summary>
    /// Preferred product categories
    /// </summary>
    public List<string> PreferredCategories { get; set; } = new();
}

/// <summary>
/// Customer preferences and behavioral data
/// </summary>
public class CustomerPreferencesDto
{
    /// <summary>
    /// Preferred communication method
    /// </summary>
    public string PreferredContact { get; set; } = string.Empty;
    
    /// <summary>
    /// Marketing email subscription status
    /// </summary>
    public bool EmailOptIn { get; set; }
    
    /// <summary>
    /// Customer tier or loyalty level
    /// </summary>
    public string LoyaltyTier { get; set; } = string.Empty;
    
    /// <summary>
    /// Special notes or requirements
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Tags for customer segmentation
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Customer segmentation information
/// </summary>
public class CustomerSegmentDto
{
    /// <summary>
    /// Segment name
    /// </summary>
    [Required]
    public string SegmentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Segment description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of customers in this segment
    /// </summary>
    public int CustomerCount { get; set; }
    
    /// <summary>
    /// Percentage of total customer base
    /// </summary>
    public decimal Percentage { get; set; }
    
    /// <summary>
    /// Average value per customer in this segment
    /// </summary>
    public decimal AverageValue { get; set; }
}

/// <summary>
/// Customer distribution by geographic region
/// </summary>
public class CustomerByRegionDto
{
    /// <summary>
    /// Region name (state, country, etc.)
    /// </summary>
    [Required]
    public string Region { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of customers in this region
    /// </summary>
    public int CustomerCount { get; set; }
    
    /// <summary>
    /// Percentage of total customer base
    /// </summary>
    public decimal Percentage { get; set; }
    
    /// <summary>
    /// Total revenue from this region
    /// </summary>
    public decimal TotalRevenue { get; set; }
}
