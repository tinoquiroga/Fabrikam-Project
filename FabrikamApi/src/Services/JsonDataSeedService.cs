using FabrikamApi.Data;
using FabrikamApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FabrikamApi.Services;

public class JsonDataSeedService : ISeedService
{
    private readonly FabrikamDbContext _context;
    private readonly ILogger<JsonDataSeedService> _logger;
    private readonly IWebHostEnvironment _environment;

    public JsonDataSeedService(FabrikamDbContext context, ILogger<JsonDataSeedService> logger, IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    public async Task SeedDataAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await _context.Customers.AnyAsync())
            {
                _logger.LogInformation("Database already contains data, skipping seed");
                return;
            }

            _logger.LogInformation("Starting JSON-based database seed...");

            await SeedProductsFromJson();
            await SeedCustomersFromJson();
            await SeedOrdersFromJson();

            await _context.SaveChangesAsync();
            _logger.LogInformation("JSON-based database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during JSON-based database seeding");
            throw;
        }
    }

    public async Task ForceReseedAsync()
    {
        try
        {
            _logger.LogInformation("Starting force re-seed of database...");

            // Clear existing data
            _context.Orders.RemoveRange(_context.Orders);
            _context.Customers.RemoveRange(_context.Customers);
            _context.Products.RemoveRange(_context.Products);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleared existing data, now re-seeding...");

            await SeedProductsFromJson();
            await SeedCustomersFromJson();
            await SeedOrdersFromJson();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Force re-seed completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during force re-seed");
            throw;
        }
    }

    private async Task SeedProductsFromJson()
    {
        var jsonPath = Path.Combine(_environment.ContentRootPath, "Data", "SeedData", "products.json");

        if (!File.Exists(jsonPath))
        {
            _logger.LogWarning("Products JSON file not found at {Path}", jsonPath);
            return;
        }

        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        var productsData = JsonSerializer.Deserialize<List<ProductSeedData>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (productsData == null)
        {
            _logger.LogWarning("Failed to deserialize products data");
            return;
        }

        var products = productsData.Select(p => new Product
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            ModelNumber = p.ModelNumber,
            Category = Enum.Parse<ProductCategory>(p.Category),
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ReorderLevel = p.ReorderLevel,
            Dimensions = p.Dimensions,
            SquareFeet = p.SquareFeet,
            Bedrooms = p.Bedrooms,
            Bathrooms = p.Bathrooms,
            DeliveryDaysEstimate = p.DeliveryDaysEstimate,
            IsActive = p.IsActive
        }).ToList();

        _context.Products.AddRange(products);
        _logger.LogInformation("Added {Count} products from JSON seed data", products.Count);
    }

    private async Task SeedCustomersFromJson()
    {
        var jsonPath = Path.Combine(_environment.ContentRootPath, "Data", "SeedData", "customers.json");

        if (!File.Exists(jsonPath))
        {
            _logger.LogWarning("Customers JSON file not found at {Path}", jsonPath);
            return;
        }

        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        var customersData = JsonSerializer.Deserialize<List<CustomerSeedData>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (customersData == null)
        {
            _logger.LogWarning("Failed to deserialize customers data");
            return;
        }

        var customers = customersData.Select(c => new Customer
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            City = c.City,
            State = c.State,
            ZipCode = c.ZipCode,
            Region = c.Region,
            CreatedDate = c.CreatedDate
        }).ToList();

        _context.Customers.AddRange(customers);
        _logger.LogInformation("Added {Count} customers from JSON seed data", customers.Count);
    }

    private async Task SeedOrdersFromJson()
    {
        var jsonPath = Path.Combine(_environment.ContentRootPath, "Data", "SeedData", "orders.json");

        if (!File.Exists(jsonPath))
        {
            _logger.LogWarning("Orders JSON file not found at {Path}", jsonPath);
            return;
        }

        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        var ordersData = JsonSerializer.Deserialize<List<OrderSeedData>>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (ordersData == null)
        {
            _logger.LogWarning("Failed to deserialize orders data");
            return;
        }

        var orders = new List<Order>();
        var orderItems = new List<OrderItem>();

        foreach (var orderData in ordersData)
        {
            var order = new Order
            {
                Id = orderData.Id,
                OrderNumber = orderData.OrderNumber,
                CustomerId = orderData.CustomerId,
                OrderDate = orderData.OrderDate,
                Status = Enum.Parse<OrderStatus>(orderData.Status),
                ShippingAddress = orderData.ShippingAddress,
                ShippingCity = orderData.ShippingCity,
                ShippingState = orderData.ShippingState,
                ShippingZipCode = orderData.ShippingZip
            };

            orders.Add(order);

            // Add order items
            foreach (var itemData in orderData.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = orderData.Id,
                    ProductId = itemData.ProductId,
                    Quantity = itemData.Quantity,
                    UnitPrice = itemData.UnitPrice
                };
                orderItems.Add(orderItem);
            }
        }

        _context.Orders.AddRange(orders);
        _context.OrderItems.AddRange(orderItems);
        _logger.LogInformation("Added {OrderCount} orders with {ItemCount} items from JSON seed data", orders.Count, orderItems.Count);
    }
}

// Data transfer objects for JSON deserialization
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
