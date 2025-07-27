using System.Text.Json;
using FluentAssertions;

namespace FabrikamTests.Helpers;

/// <summary>
/// Helper class for loading and validating against JSON seed data
/// Provides methods to test API responses against known seed data
/// </summary>
public static class SeedDataHelper
{
    private static readonly string SeedDataPath = Path.Combine(
        Directory.GetCurrentDirectory(), "..", "..", "..", "..", "FabrikamApi", "src", "Data", "SeedData");

    /// <summary>
    /// Load all products from the seed data JSON file
    /// </summary>
    public static async Task<List<ProductSeedData>> LoadProductsAsync()
    {
        var filePath = Path.Combine(SeedDataPath, "products.json");
        var jsonContent = await File.ReadAllTextAsync(filePath);

        var products = JsonSerializer.Deserialize<List<ProductSeedData>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return products ?? new List<ProductSeedData>();
    }

    /// <summary>
    /// Load all customers from the seed data JSON file
    /// </summary>
    public static async Task<List<CustomerSeedData>> LoadCustomersAsync()
    {
        var filePath = Path.Combine(SeedDataPath, "customers.json");
        var jsonContent = await File.ReadAllTextAsync(filePath);

        var customers = JsonSerializer.Deserialize<List<CustomerSeedData>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return customers ?? new List<CustomerSeedData>();
    }

    /// <summary>
    /// Load all orders from the seed data JSON file
    /// </summary>
    public static async Task<List<OrderSeedData>> LoadOrdersAsync()
    {
        var filePath = Path.Combine(SeedDataPath, "orders.json");
        var jsonContent = await File.ReadAllTextAsync(filePath);

        var orders = JsonSerializer.Deserialize<List<OrderSeedData>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return orders ?? new List<OrderSeedData>();
    }

    /// <summary>
    /// Get a product by ID from seed data - useful for testing specific scenarios
    /// </summary>
    public static async Task<ProductSeedData?> GetProductByIdAsync(int id)
    {
        var products = await LoadProductsAsync();
        return products.FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// Get a customer by ID from seed data - useful for testing specific scenarios
    /// </summary>
    public static async Task<CustomerSeedData?> GetCustomerByIdAsync(int id)
    {
        var customers = await LoadCustomersAsync();
        return customers.FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Get an order by ID from seed data - useful for testing specific scenarios
    /// </summary>
    public static async Task<OrderSeedData?> GetOrderByIdAsync(int id)
    {
        var orders = await LoadOrdersAsync();
        return orders.FirstOrDefault(o => o.Id == id);
    }

    /// <summary>
    /// Get all products in a specific category
    /// </summary>
    public static async Task<List<ProductSeedData>> GetProductsByCategoryAsync(string category)
    {
        var products = await LoadProductsAsync();
        return products.Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Get all customers in a specific region
    /// </summary>
    public static async Task<List<CustomerSeedData>> GetCustomersByRegionAsync(string region)
    {
        var customers = await LoadCustomersAsync();
        return customers.Where(c => string.Equals(c.Region, region, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Get all orders with a specific status
    /// </summary>
    public static async Task<List<OrderSeedData>> GetOrdersByStatusAsync(string status)
    {
        var orders = await LoadOrdersAsync();
        return orders.Where(o => string.Equals(o.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Validate that an API response contains all expected seed data
    /// </summary>
    public static async Task ValidateProductListResponse(List<object> apiResponse)
    {
        var seedProducts = await LoadProductsAsync();

        apiResponse.Should().HaveCount(seedProducts.Count,
            "API should return all seed products");

        // Validate each product exists in seed data
        foreach (var apiProduct in apiResponse)
        {
            var productJson = JsonSerializer.Serialize(apiProduct);
            var productData = JsonSerializer.Deserialize<ProductSeedData>(productJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            productData.Should().NotBeNull();

            var expectedProduct = seedProducts.FirstOrDefault(p => p.Id == productData!.Id);
            expectedProduct.Should().NotBeNull($"Product with ID {productData!.Id} should exist in seed data");

            // Validate key properties match
            productData.Name.Should().Be(expectedProduct!.Name);
            productData.ModelNumber.Should().Be(expectedProduct.ModelNumber);
            productData.Price.Should().Be(expectedProduct.Price);
        }
    }

    /// <summary>
    /// Validate that a customer response matches seed data
    /// </summary>
    public static async Task ValidateCustomerResponse(object apiCustomer, int expectedId)
    {
        var seedCustomer = await GetCustomerByIdAsync(expectedId);
        seedCustomer.Should().NotBeNull($"Customer with ID {expectedId} should exist in seed data");

        var customerJson = JsonSerializer.Serialize(apiCustomer);
        var customerData = JsonSerializer.Deserialize<CustomerSeedData>(customerJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customerData.Should().NotBeNull();
        customerData!.FirstName.Should().Be(seedCustomer!.FirstName);
        customerData.LastName.Should().Be(seedCustomer.LastName);
        customerData.Email.Should().Be(seedCustomer.Email);
        customerData.Region.Should().Be(seedCustomer.Region);
    }

    /// <summary>
    /// Get IDs that don't exist in seed data - useful for testing 404 scenarios
    /// </summary>
    public static async Task<int> GetNonExistentProductIdAsync()
    {
        var products = await LoadProductsAsync();
        var maxId = products.Max(p => p.Id);
        return maxId + 100; // Return an ID that definitely doesn't exist
    }

    /// <summary>
    /// Get IDs that don't exist in seed data - useful for testing 404 scenarios
    /// </summary>
    public static async Task<int> GetNonExistentCustomerIdAsync()
    {
        var customers = await LoadCustomersAsync();
        var maxId = customers.Max(c => c.Id);
        return maxId + 100; // Return an ID that definitely doesn't exist
    }

    /// <summary>
    /// Get IDs that don't exist in seed data - useful for testing 404 scenarios
    /// </summary>
    public static async Task<int> GetNonExistentOrderIdAsync()
    {
        var orders = await LoadOrdersAsync();
        var maxId = orders.Max(o => o.Id);
        return maxId + 100; // Return an ID that definitely doesn't exist
    }

    /// <summary>
    /// Calculate expected sales analytics from seed data
    /// Useful for validating analytics endpoints
    /// </summary>
    public static async Task<SalesAnalyticsSeedData> CalculateExpectedSalesAnalyticsAsync()
    {
        var orders = await LoadOrdersAsync();
        var products = await LoadProductsAsync();

        var totalOrders = orders.Count;
        var totalRevenue = orders.SelectMany(o => o.Items).Sum(i => i.UnitPrice * i.Quantity);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var statusBreakdown = orders
            .GroupBy(o => o.Status)
            .Select(g => new StatusBreakdown
            {
                Status = g.Key,
                Count = g.Count(),
                Revenue = g.SelectMany(o => o.Items).Sum(i => i.UnitPrice * i.Quantity)
            }).ToList();

        // Calculate region breakdown based on customers
        var customers = await LoadCustomersAsync();
        var regionBreakdown = orders
            .Join(customers, o => o.CustomerId, c => c.Id, (o, c) => new { Order = o, Customer = c })
            .GroupBy(x => x.Customer.Region)
            .Select(g => new RegionBreakdown
            {
                Region = g.Key,
                Count = g.Count(),
                Revenue = g.SelectMany(x => x.Order.Items).Sum(i => i.UnitPrice * i.Quantity)
            }).ToList();

        return new SalesAnalyticsSeedData
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            AverageOrderValue = averageOrderValue,
            StatusBreakdown = statusBreakdown,
            RegionBreakdown = regionBreakdown
        };
    }
}

// Data classes for seed data validation
public class ProductSeedData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ModelNumber { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public string? Dimensions { get; set; }
    public int? SquareFeet { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int DeliveryDaysEstimate { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerSeedData
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string Region { get; set; } = "";
    public DateTime CreatedDate { get; set; }
}

// DTO to match API customer response format
public class ApiCustomerData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public AddressData Address { get; set; } = new();
    public string Region { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public OrderSummaryData OrderSummary { get; set; } = new();
    public List<object> RecentOrders { get; set; } = new();
}

public class AddressData
{
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
}

public class OrderSummaryData
{
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

public class OrderSeedData
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public string ShippingCity { get; set; } = "";
    public string ShippingState { get; set; } = "";
    public string ShippingZip { get; set; } = "";
    public List<OrderItemSeedData> Items { get; set; } = new();
}

public class OrderItemSeedData
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SalesAnalyticsSeedData
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<StatusBreakdown> StatusBreakdown { get; set; } = new();
    public List<RegionBreakdown> RegionBreakdown { get; set; } = new();
}

// DTO to match API analytics response format
public class ApiSalesAnalyticsData
{
    public SalesSummaryData Summary { get; set; } = new();
    public List<SalesByStatusData> ByStatus { get; set; } = new();
    public List<SalesByRegionData> ByRegion { get; set; } = new();
    public List<SalesTrendData> RecentTrends { get; set; } = new();
}

public class SalesSummaryData
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public SalesPeriodData Period { get; set; } = new();
}

public class SalesPeriodData
{
    public string FromDate { get; set; } = "";
    public string ToDate { get; set; } = "";
}

public class SalesByStatusData
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class SalesByRegionData
{
    public string Region { get; set; } = "";
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class SalesTrendData
{
    public string Date { get; set; } = "";
    public int Orders { get; set; }
    public decimal Revenue { get; set; }
}

public class StatusBreakdown
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class RegionBreakdown
{
    public string Region { get; set; } = "";
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}
