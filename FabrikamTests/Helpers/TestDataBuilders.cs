using FabrikamContracts.DTOs;
using FabrikamContracts.DTOs.Customers;
using FabrikamContracts.DTOs.Orders;
using FabrikamContracts.DTOs.Products;
using System.Text.Json;

namespace FabrikamTests.Helpers;

/// <summary>
/// Test data builders using fluent pattern for consistent test data creation
/// Supports Given/When/Then test structure with easy-to-read test setup
/// </summary>
public static class TestDataBuilders
{
    public static class Given
    {
        public static CustomerBuilder ACustomer() => new CustomerBuilder();
        public static OrderBuilder AnOrder() => new OrderBuilder();
        public static ProductBuilder AProduct() => new ProductBuilder();
        public static AuthenticationSettingsBuilder AuthenticationSettings() => new AuthenticationSettingsBuilder();
        public static ServiceJwtSettingsBuilder ServiceJwtSettings() => new ServiceJwtSettingsBuilder();
    }

    // Direct static methods for convenience
    public static CustomerBuilder Customer() => new CustomerBuilder();
    public static OrderBuilder Order() => new OrderBuilder();
    public static ProductBuilder Product() => new ProductBuilder();
    public static AuthenticationSettingsBuilder AuthenticationSettings() => new AuthenticationSettingsBuilder();
    public static ServiceJwtSettingsBuilder ServiceJwtSettings() => new ServiceJwtSettingsBuilder();
}

/// <summary>
/// Fluent builder for creating test customer data
/// </summary>
public class CustomerBuilder
{
    private int _id = 1;
    private string _name = "Test Customer";
    private string _email = "test@example.com";
    private string _phone = "+1 555 0123";
    private string _city = "Seattle";
    private string _state = "WA";
    private string _region = "Pacific Northwest";
    private DateTime _createdDate = DateTime.UtcNow;
    private int _orderCount = 0;
    private decimal _totalSpent = 0m;

    public CustomerBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public CustomerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CustomerBuilder WithRegion(string region)
    {
        _region = region;
        return this;
    }

    public CustomerBuilder WithOrderCount(int orderCount)
    {
        _orderCount = orderCount;
        return this;
    }

    public CustomerBuilder WithTotalSpent(decimal totalSpent)
    {
        _totalSpent = totalSpent;
        return this;
    }

    public CustomerBuilder InRegion(string region)
    {
        _region = region;
        return this;
    }

    public CustomerBuilder AsHighValueCustomer()
    {
        _orderCount = 10;
        _totalSpent = 50000m;
        return this;
    }

    public CustomerBuilder AsNewCustomer()
    {
        _orderCount = 0;
        _totalSpent = 0m;
        _createdDate = DateTime.UtcNow.AddDays(-1);
        return this;
    }

    public CustomerListItemDto Build()
    {
        return new CustomerListItemDto
        {
            Id = _id,
            Name = _name,
            Email = _email,
            Phone = _phone,
            City = _city,
            State = _state,
            Region = _region,
            CreatedDate = _createdDate,
            OrderCount = _orderCount,
            TotalSpent = _totalSpent
        };
    }

    public string BuildAsJson()
    {
        return JsonSerializer.Serialize(Build(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}

/// <summary>
/// Fluent builder for creating test order data
/// </summary>
public class OrderBuilder
{
    private int _id = 1;
    private int _customerId = 1;
    private DateTime _orderDate = DateTime.UtcNow;
    private string _status = "Pending";
    private decimal _total = 1000m;
    private List<object> _items = new();

    public OrderBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public OrderBuilder ForCustomer(int customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public OrderBuilder WithTotal(decimal total)
    {
        _total = total;
        return this;
    }

    public OrderBuilder WithItem(int productId, int quantity = 1, decimal unitPrice = 100m)
    {
        _items.Add(new 
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        });
        return this;
    }

    public OrderBuilder AsCompletedOrder()
    {
        _status = "Completed";
        _orderDate = DateTime.UtcNow.AddDays(-7);
        return this;
    }

    public OrderBuilder AsPendingOrder()
    {
        _status = "Pending";
        _orderDate = DateTime.UtcNow.AddHours(-2);
        return this;
    }

    public OrderDto Build()
    {
        return new OrderDto
        {
            Id = _id,
            Customer = new OrderCustomerDto { Id = _customerId },
            OrderDate = _orderDate,
            Status = _status,
            Total = _total
        };
    }
}

/// <summary>
/// Fluent builder for creating test product data
/// </summary>
public class ProductBuilder
{
    private int _id = 1;
    private string _name = "Test Product";
    private decimal _price = 100m;
    private string _category = "Base";
    private int _stockQuantity = 10;

    public ProductBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ProductBuilder WithCategory(string category)
    {
        _category = category;
        return this;
    }

    public ProductBuilder WithStock(int quantity)
    {
        _stockQuantity = quantity;
        return this;
    }

    public ProductBuilder AsOutOfStock()
    {
        _stockQuantity = 0;
        return this;
    }

    public ProductBuilder AsHighValueProduct()
    {
        _price = 5000m;
        _category = "Premium";
        return this;
    }

    public ProductDto Build()
    {
        return new ProductDto
        {
            Id = _id,
            Name = _name,
            Category = _category,
            Price = _price,
            StockQuantity = _stockQuantity,
            ModelNumber = $"MODEL-{_id}",
            ReorderLevel = 5,
            StockStatus = _stockQuantity > 0 ? "In Stock" : "Out of Stock",
            DeliveryDaysEstimate = 14
        };
    }
}

/// <summary>
/// Fluent builder for authentication settings
/// </summary>
public class AuthenticationSettingsBuilder
{
    private AuthenticationMode _mode = AuthenticationMode.BearerToken;
    private JwtSettings _jwtSettings = new();
    private ServiceJwtSettings _serviceJwtSettings = new();
    private GuidValidationSettings _guidValidationSettings = new();

    public AuthenticationSettingsBuilder WithMode(AuthenticationMode mode)
    {
        _mode = mode;
        return this;
    }

    public AuthenticationSettingsBuilder AsDisabled()
    {
        _mode = AuthenticationMode.Disabled;
        return this;
    }

    public AuthenticationSettingsBuilder AsBearerToken()
    {
        _mode = AuthenticationMode.BearerToken;
        return this;
    }

    public AuthenticationSettingsBuilder AsEntraExternalId()
    {
        _mode = AuthenticationMode.EntraExternalId;
        return this;
    }

    public AuthenticationSettingsBuilder WithJwtSettings(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
        return this;
    }

    public AuthenticationSettingsBuilder WithServiceJwtSettings(ServiceJwtSettings serviceJwtSettings)
    {
        _serviceJwtSettings = serviceJwtSettings;
        return this;
    }

    public AuthenticationSettings Build()
    {
        return new AuthenticationSettings
        {
            Mode = _mode,
            Jwt = _jwtSettings,
            ServiceJwt = _serviceJwtSettings,
            GuidValidation = _guidValidationSettings
        };
    }
}

/// <summary>
/// Fluent builder for service JWT settings
/// </summary>
public class ServiceJwtSettingsBuilder
{
    private string _secretKey = "TestSecretKeyForServiceJwtThatIsLongEnoughForHS256Algorithm";
    private string _issuer = "FabrikamTestIssuer";
    private string _audience = "FabrikamTestAudience";
    private int _expiryMinutes = 60;
    private string _serviceName = "TestService";
    private Dictionary<string, string> _environmentVariables = new();

    public ServiceJwtSettingsBuilder WithSecretKey(string secretKey)
    {
        _secretKey = secretKey;
        return this;
    }

    public ServiceJwtSettingsBuilder WithIssuer(string issuer)
    {
        _issuer = issuer;
        return this;
    }

    public ServiceJwtSettingsBuilder WithAudience(string audience)
    {
        _audience = audience;
        return this;
    }

    public ServiceJwtSettingsBuilder WithTokenLifetimeInMinutes(int minutes)
    {
        _expiryMinutes = minutes;
        return this;
    }

    public ServiceJwtSettingsBuilder WithExpiryMinutes(int minutes)
    {
        _expiryMinutes = minutes;
        return this;
    }

    public ServiceJwtSettingsBuilder WithServiceName(string serviceName)
    {
        _serviceName = serviceName;
        return this;
    }

    public ServiceJwtSettingsBuilder WithDefaults()
    {
        _secretKey = "TestSecretKeyForServiceJwtThatIsLongEnoughForHS256Algorithm";
        _issuer = "FabrikamTestIssuer";
        _audience = "FabrikamTestAudience";
        _expiryMinutes = 60;
        return this;
    }

    public ServiceJwtSettingsBuilder WithEnvironmentVariable(string key, string value)
    {
        _environmentVariables[key] = value;
        return this;
    }

    public ServiceJwtSettingsBuilder WithClearEnvironmentVariables()
    {
        _environmentVariables.Clear();
        return this;
    }

    public ServiceJwtSettings Build()
    {
        var settings = new ServiceJwtSettings
        {
            SecretKey = _secretKey,
            Issuer = _issuer,
            Audience = _audience,
            ExpirationMinutes = _expiryMinutes,
            ServiceName = _serviceName
        };

        // Store environment variables in test context for testing
        if (_environmentVariables.Any())
        {
            // In real scenarios, this would interact with Environment.GetEnvironmentVariable
            // For testing, we'll store it in the test context
        }

        return settings;
    }
}
