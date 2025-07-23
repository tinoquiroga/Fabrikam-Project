# 🏗️ Fabrikam Project - Copilot Development Instructions

## 📋 Project Overview
This is a .NET 9.0 business simulation platform with two main components:
- **FabrikamApi**: ASP.NET Core Web API for modular homes business operations
- **FabrikamMcp**: Model Context Protocol server enabling AI integration

## 🎯 C# Best Practices & Coding Standards

### ⚡ Asynchronous Programming
**ALWAYS use async/await patterns for:**
- Database operations (Entity Framework)
- HTTP client calls
- File I/O operations
- Any potentially blocking operations

```csharp
// ✅ CORRECT: Async database operations
public async Task<ActionResult<Customer>> GetCustomer(int id)
{
    var customer = await _context.Customers
        .Include(c => c.Orders)
        .FirstOrDefaultAsync(c => c.Id == id);
    
    return customer != null ? Ok(customer) : NotFound();
}

// ❌ INCORRECT: Synchronous database operations
public ActionResult<Customer> GetCustomer(int id)
{
    var customer = _context.Customers.Find(id); // Blocking call
    return Ok(customer);
}
```

### 🛡️ Error Handling & Logging
**Implement comprehensive error handling:**

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Product>> GetProduct(int id)
{
    try
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found");
        }
        
        return Ok(product);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving product {ProductId}", id);
        return StatusCode(500, "An error occurred while retrieving the product");
    }
}
```

**Key principles:**
- ✅ Always log exceptions with structured logging parameters
- ✅ Return appropriate HTTP status codes
- ✅ Provide meaningful error messages to clients
- ✅ Never expose internal exception details to clients
- ✅ Use try-catch blocks around database operations

### 🔍 Entity Framework Best Practices

**Query Optimization:**
```csharp
// ✅ CORRECT: Use Include for related data, projection for specific fields
var orders = await _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
    .Where(o => o.Status == OrderStatus.Pending)
    .Select(o => new OrderDto 
    { 
        Id = o.Id, 
        CustomerName = o.Customer.FirstName + " " + o.Customer.LastName,
        Total = o.Total 
    })
    .ToListAsync();

// ❌ INCORRECT: Loading all data unnecessarily
var orders = _context.Orders.ToList(); // Loads everything synchronously
```

**Database Operations:**
- ✅ Use `FindAsync()` for single entity by primary key
- ✅ Use `FirstOrDefaultAsync()` for single entity with conditions
- ✅ Use `AnyAsync()` for existence checks
- ✅ Use `CountAsync()` for counting records
- ✅ Always use `SaveChangesAsync()` instead of `SaveChanges()`

### 📊 API Controller Patterns

**Standard Controller Structure:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly FabrikamDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(FabrikamDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] ProductCategory? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Products.AsQueryable();
            
            // Apply filters
            if (category.HasValue)
            {
                query = query.Where(p => p.Category == category.Value);
            }
            
            // Implement pagination
            var totalCount = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Add pagination headers
            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());
            
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products with category {Category}", category);
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }
}
```

### 🔐 Validation & Business Rules

**Input Validation:**
```csharp
[HttpPost]
public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequest request)
{
    try
    {
        // Validate required business rules
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer == null)
        {
            return BadRequest($"Customer with ID {request.CustomerId} not found");
        }

        // Validate stock availability
        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {item.ProductId} not found");
            }
            
            if (product.StockQuantity < item.Quantity)
            {
                return BadRequest($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
            }
        }

        // Create and save order
        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            OrderItems = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new order {OrderId} for customer {CustomerId}", 
            order.Id, request.CustomerId);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating order for customer {CustomerId}", request.CustomerId);
        return StatusCode(500, "An error occurred while creating the order");
    }
}
```

### 🌐 HTTP Client Best Practices (MCP Tools)

**HttpClient Usage in MCP Tools:**
```csharp
[McpServerTool, Description("Get customer analytics with proper error handling")]
public async Task<string> GetCustomerAnalytics(string? region = null)
{
    try
    {
        var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(region)) 
        {
            queryParams.Add($"region={Uri.EscapeDataString(region)}");
        }
        
        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var response = await _httpClient.GetAsync($"{baseUrl}/api/customers/analytics{queryString}");
        
        if (response.IsSuccessStatusCode)
        {
            var analytics = await response.Content.ReadAsStringAsync();
            return $"Customer Analytics for {region ?? "All Regions"}:\n{analytics}";
        }
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return "No customer analytics data found for the specified criteria";
        }
        
        return $"Error retrieving customer analytics: {response.StatusCode} - {response.ReasonPhrase}";
    }
    catch (HttpRequestException ex)
    {
        return $"Network error retrieving customer analytics: {ex.Message}";
    }
    catch (Exception ex)
    {
        return $"Error retrieving customer analytics: {ex.Message}";
    }
}
```

### 🏗️ Dependency Injection & Configuration

**Service Registration:**
```csharp
// ✅ CORRECT: Register services with appropriate lifetimes
builder.Services.AddScoped<DataSeedService>();
builder.Services.AddDbContext<FabrikamDbContext>(options =>
    options.UseInMemoryDatabase("FabrikamDb"));
builder.Services.AddHttpClient<FabrikamSalesTools>();

// ✅ CORRECT: Configuration binding
builder.Services.Configure<FabrikamApiSettings>(
    builder.Configuration.GetSection("FabrikamApi"));
```

### 📝 Documentation & XML Comments

**API Documentation:**
```csharp
/// <summary>
/// Get all products with optional filtering and pagination
/// </summary>
/// <param name="category">Filter by product category</param>
/// <param name="inStock">Filter by stock availability</param>
/// <param name="page">Page number (1-based)</param>
/// <param name="pageSize">Number of items per page (max 100)</param>
/// <returns>Paginated list of products</returns>
/// <response code="200">Returns the filtered product list</response>
/// <response code="400">Invalid filter parameters</response>
/// <response code="500">Internal server error</response>
[HttpGet]
[ProducesResponseType(typeof(IEnumerable<Product>), 200)]
[ProducesResponseType(400)]
[ProducesResponseType(500)]
public async Task<ActionResult<IEnumerable<Product>>> GetProducts(...)
```

### 🧪 Testing Patterns

**Unit Test Structure:**
```csharp
[Test]
public async Task GetProduct_WithValidId_ReturnsProduct()
{
    // Arrange
    var productId = 1;
    var expectedProduct = new Product { Id = productId, Name = "Test Product" };
    
    // Act
    var result = await _controller.GetProduct(productId);
    
    // Assert
    var okResult = result.Result as OkObjectResult;
    Assert.IsNotNull(okResult);
    var product = okResult.Value as Product;
    Assert.AreEqual(expectedProduct.Id, product.Id);
}

[Test]
public async Task GetProduct_WithInvalidId_ReturnsNotFound()
{
    // Arrange
    var invalidId = 999;
    
    // Act
    var result = await _controller.GetProduct(invalidId);
    
    // Assert
    Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
}
```

## 🔧 Project-Specific Guidelines

### Database Context Usage
- ✅ Always use `FabrikamDbContext` for database operations
- ✅ Include related entities when needed for business operations
- ✅ Use projection for read-only operations to improve performance
- ✅ Implement proper transaction handling for complex operations

### API Response Patterns
- ✅ Return `ActionResult<T>` for API methods
- ✅ Use appropriate HTTP status codes (200, 201, 400, 404, 500)
- ✅ Include pagination headers for list endpoints
- ✅ Provide meaningful error messages

### MCP Tool Development
- ✅ All MCP tools must be async and return Task<string>
- ✅ Use proper HttpClient error handling
- ✅ Escape query parameters with `Uri.EscapeDataString()`
- ✅ Provide descriptive tool descriptions for AI understanding
- ✅ Handle API failures gracefully with user-friendly messages

### Logging Standards
- ✅ Use structured logging with parameters
- ✅ Log at appropriate levels (Information, Warning, Error)
- ✅ Include relevant context (IDs, parameters, operation types)
- ✅ Never log sensitive information (passwords, API keys)

## 🚫 Common Anti-Patterns to Avoid

### ❌ Synchronous Database Operations
```csharp
// DON'T DO THIS
var customer = _context.Customers.Find(id); // Blocking
var customers = _context.Customers.ToList(); // Loads all data
```

### ❌ Missing Error Handling
```csharp
// DON'T DO THIS
[HttpGet("{id}")]
public async Task<Customer> GetCustomer(int id)
{
    return await _context.Customers.FindAsync(id); // No error handling
}
```

### ❌ Exposing Internal Exceptions
```csharp
// DON'T DO THIS
catch (Exception ex)
{
    return BadRequest(ex.Message); // Exposes internal details
}
```

### ❌ N+1 Query Problems
```csharp
// DON'T DO THIS
var orders = await _context.Orders.ToListAsync();
foreach (var order in orders)
{
    var customer = await _context.Customers.FindAsync(order.CustomerId); // N+1 queries
}
```

## 🎨 Code Style Guidelines

### Naming Conventions
- ✅ Use PascalCase for public methods, properties, and classes
- ✅ Use camelCase for parameters and private fields
- ✅ Use descriptive names that indicate purpose
- ✅ Prefix private fields with underscore (_context, _logger)

### Method Organization
- ✅ Group related methods together
- ✅ Public methods before private methods
- ✅ CRUD operations in logical order (GET, POST, PUT, PATCH, DELETE)
- ✅ Helper methods at the bottom of the class

### File Organization
- ✅ One controller per file
- ✅ DTOs in the same file as the controller that uses them
- ✅ Models in dedicated Models folder
- ✅ Services in dedicated Services folder

## 🔄 Continuous Improvement

### Performance Optimization
- ✅ Use async/await consistently
- ✅ Implement proper query optimization
- ✅ Add database indexes for frequently queried fields
- ✅ Use caching for read-heavy operations when appropriate

### Security Considerations
- ✅ Validate all input parameters
- ✅ Use parameterized queries (Entity Framework handles this)
- ✅ Implement proper authorization (add when moving to production)
- ✅ Never expose sensitive information in error messages

### Maintainability
- ✅ Write self-documenting code with clear method names
- ✅ Keep methods focused on single responsibilities
- ✅ Use constants for magic numbers and strings
- ✅ Implement consistent error handling patterns

---

**Remember**: This is a demonstration platform, so prioritize clarity and educational value while maintaining production-quality code patterns. Always use async/await, proper error handling, and structured logging to showcase modern C# best practices.
