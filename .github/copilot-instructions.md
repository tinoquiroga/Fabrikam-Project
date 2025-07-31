# 🏗️ Fabrikam Project - GitHub Copilot Development Instructions

## 🚨 **CRITICAL: GitHub Copilot Server Management Rules**

**⚠️ ALWAYS FOLLOW THESE RULES TO AVOID BREAKING SERVERS ⚠️**

### **🎯 Rule #1: Use VS Code Tasks for Servers**
```markdown
✅ DO: Use `run_task` tool for server management
❌ DON'T: Use `run_in_terminal` with `dotnet run` commands

Correct pattern:
- Start: run_task("🌐 Start Both Servers")
- Stop: run_task("🛑 Stop All Servers") 
- Status: run_task("📊 Project Status")
```

### **🎯 Rule #2: Use Separate Terminals for Commands**
```markdown
✅ DO: Use `run_in_terminal` (isBackground: false) for testing commands
❌ DON'T: Run commands in server terminals (stops servers)

Safe commands in separate terminals:
- curl -k https://localhost:7297/api/info
- Invoke-RestMethod -SkipCertificateCheck
- ./test.ps1 -Quick
- dotnet test
```

### **🎯 Rule #3: Never Mix Servers and Commands**
```markdown
DEDICATED SERVER TERMINALS: Only for servers (via VS Code tasks)
COMMAND TERMINALS: Only for testing/commands (via run_in_terminal)
NEVER run commands where servers are running!
```

**📖 Full Strategy**: `docs/development/GITHUB-COPILOT-SERVER-STRATEGY.md`

---

## 📋 Project Overview

This is a .NET 9.0 business simulation platform with two main components:

- **FabrikamApi**: ASP.NET Core Web API for modular homes business operations
- **FabrikamMcp**: Model Context Protocol server enabling AI integration

## ☁️ **Azure Environment Configuration**

**This project uses Microsoft Azure for cloud deployment and services.**

### **Primary Azure Subscription**

- **Subscription**: MCAPS-Hybrid-REQ-59531-2023-davidb
- **Subscription ID**: `1ae622b1-c33c-457f-a2bb-351fed78922f`
- **Tenant**: Microsoft Non-Production (fpdo.microsoft.com)
- **Tenant ID**: `16b3c013-d300-468d-ac64-7eda0820b6d3`
- **Primary Resource Group**: `rg-fabrikam-dev` (instance-specific)

### **Azure Services in Use**

- **Authentication**: Dual strategy (Entra External ID preferred, fallback to ASP.NET Core Identity)
- **Azure App Service**: Web API hosting
- **Azure Container Apps**: MCP server hosting
- **Azure Key Vault**: Secrets management
- **Azure Application Insights**: Monitoring and telemetry
- **Azure SQL Database**: User and business data storage

**When working with Azure resources:**

- Always use the correct subscription and tenant context
- Reference the rg-fabrikam-dev resource group for new resources
- Use managed identity authentication where possible
- Store secrets in Azure Key Vault, never in code
- Support multi-instance deployment with instance-specific resource groups

## 🔄 **CI/CD Integration Awareness**pilot Development Instructions

## � **CRITICAL: Monorepo Structure Awareness**

**This is a MONOREPO with multiple .NET projects. See `.github/MONOREPO-GUIDE.md` for complete details.**

### 🎯 **Quick Monorepo Rules**

- **ALWAYS** work from workspace root: `c:\Users\davidb\1Repositories\Fabrikam-Project`
- **NEVER** navigate into individual project folders
- **USE** `--project` flag: `dotnet run --project FabrikamApi\src\FabrikamApi.csproj`
- **TWO** separate servers: API (port 7297) + MCP (port 5000)

```powershell
# ✅ CORRECT: Start both servers from workspace root
dotnet run --project FabrikamApi\src\FabrikamApi.csproj  # Terminal 1
dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj  # Terminal 2

# ❌ WRONG: Don't navigate into folders
cd FabrikamApi\src
dotnet run  # This breaks monorepo context
```

## �📋 Project Overview

This is a .NET 9.0 business simulation platform with two main components:

- **FabrikamApi**: ASP.NET Core Web API for modular homes business operations
- **FabrikamMcp**: Model Context Protocol server enabling AI integration

## 🔄 **CI/CD Integration Awareness**

**This project uses GitHub Actions for automated testing and deployment.**

- 📖 **Full CI/CD Documentation**: See `.github/copilot-cicd-context.md` for complete pipeline details
- 🧪 **Testing**: All code changes trigger automated testing via `testing.yml`
- 🚀 **Deployment**: Azure App Service deployment via `deploy-full-stack.yml`
- ✅ **Quality Gates**: Code must pass build + tests before deployment

**When suggesting code changes, always consider:**

- Will this break the automated build pipeline?
- Are tests included/updated for new functionality?
- Does this follow patterns compatible with Azure deployment?
- Are async/await patterns maintained for cloud scalability?

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

## � GitHub Copilot Server Management Strategy

**CRITICAL: Use VS Code Tasks for server management, separate terminals for commands**

### **✅ CORRECT Server Management Pattern**

```markdown
**For Server Management** (GitHub Copilot should ALWAYS use VS Code Tasks):
1. Start servers: Use `run_task` tool with "🌐 Start Both Servers"
2. Check status: Use `run_task` tool with "📊 Project Status"  
3. Stop servers: Use `run_task` tool with "🛑 Stop All Servers"
4. NEVER use: `dotnet run` commands directly in terminals

**For Commands** (GitHub Copilot should use separate terminals):
1. Use `run_in_terminal` tool with `isBackground: false` for commands
2. Safe commands: curl, Invoke-RestMethod, ./test.ps1, dotnet test
3. NEVER run commands in server terminals (breaks servers)
```

### **Available VS Code Tasks**

| Task Name | Purpose | When to Use |
|-----------|---------|-------------|
| 🚀 Start API Server | Start API on port 7297 | Individual API testing |
| 🤖 Start MCP Server | Start MCP on ports 5000/5001 | Individual MCP testing |
| 🌐 Start Both Servers | Start both servers parallel | Full stack testing |
| 🛑 Stop All Servers | Stop all running servers | Cleanup/restart |
| 🏗️ Build Solution | Build entire solution | Before starting servers |
| ⚡ Quick Test | Fast health check | Quick validation |
| 🧪 Full Tests | Comprehensive testing | Full validation |

**📖 Full Strategy**: See `docs/development/GITHUB-COPILOT-SERVER-STRATEGY.md`

## �🧪 Testing Infrastructure

This project includes a comprehensive testing infrastructure. Use these tools during development:

### **Automated Testing Scripts**

- **`test.ps1`** - Main development testing script

  ```powershell
  .\test.ps1 -Quick     # Fast health check
  .\test.ps1 -ApiOnly   # API endpoint testing
  .\test.ps1 -McpOnly   # MCP tool testing
  .\test.ps1 -Verbose   # Full detailed testing
  ```

- **`Fix-Verification.ps1`** - Quick verification after fixes
- **`api-tests.http`** - Manual API testing (use with REST Client extension)

### **Test Project Structure**

- **`FabrikamTests/`** - Automated unit and integration tests
  - `Api/OrdersControllerTests.cs` - API endpoint tests
  - `Mcp/FabrikamSalesToolsTests.cs` - MCP tool tests

### **CI/CD Testing**

- **`.github/workflows/testing.yml`** - Automated testing pipeline
- Runs on every push and pull request
- Validates API contracts and MCP protocol compliance

### **Documentation**

- **`TESTING-STRATEGY.md`** - Detailed testing methodology
- **`DEVELOPMENT-WORKFLOW.md`** - Daily workflow guide
- **`PROJECT-TESTING-SUMMARY.md`** - Complete strategy overview

Use these tools to ensure your changes don't break existing functionality!

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
- ✅ Prefix private fields with underscore (\_context, \_logger)

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

## 🚀 Code Evolution Guidelines

### **Adding New API Endpoints**

When adding new API endpoints, follow this checklist:

1. **Create the Endpoint**:

   ```csharp
   [HttpGet("{id}")]
   public async Task<ActionResult<NewResourceDto>> GetNewResource(int id)
   {
       try
       {
           var resource = await _context.NewResources.FindAsync(id);
           if (resource == null)
           {
               return NotFound($"Resource with ID {id} not found");
           }
           return Ok(resource);
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Error retrieving resource {ResourceId}", id);
           return StatusCode(500, "An error occurred while retrieving the resource");
       }
   }
   ```

2. **Add to Testing Files**:

   - Add HTTP request to `api-tests.http`
   - Add test case to `FabrikamTests/Api/`
   - Update `test.ps1` if needed

3. **Validate with Testing**:

   ```powershell
   # Test new endpoint
   .\test.ps1 -ApiOnly
   ```

4. **Update Documentation**:
   - Add XML documentation to the endpoint
   - Update API info endpoint if needed

### **Adding New MCP Tools**

When adding new MCP tools, follow this pattern:

1. **Create the Tool**:

   ```csharp
   [McpServerTool, Description("Description of what this tool does")]
   public async Task<object> NewTool(string? parameter = null)
   {
       try
       {
           var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
           var queryParams = new List<string>();

           if (!string.IsNullOrEmpty(parameter))
           {
               queryParams.Add($"param={Uri.EscapeDataString(parameter)}");
           }

           var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
           var response = await _httpClient.GetAsync($"{baseUrl}/api/endpoint{queryString}");

           if (response.IsSuccessStatusCode)
           {
               var data = await response.Content.ReadAsStringAsync();
               return new
               {
                   content = new object[]
                   {
                       new { type = "text", text = FormatResponse(data) }
                   },
                   data = data
               };
           }

           return new
           {
               error = new
               {
                   code = (int)response.StatusCode,
                   message = $"Error: {response.StatusCode} - {response.ReasonPhrase}"
               }
           };
       }
       catch (Exception ex)
       {
           return new
           {
               error = new { message = $"Error: {ex.Message}" }
           };
       }
   }
   ```

2. **Add Testing**:

   - Create test in `FabrikamTests/Mcp/`
   - Test with `.\test.ps1 -McpOnly`
   - Verify in Copilot

3. **Follow MCP Patterns**:
   - Always return structured content with `content` array
   - Handle errors gracefully
   - Use proper HTTP client patterns
   - Escape query parameters

### **Modifying DTOs/Schemas**

When changing DTOs or data schemas:

1. **Update DTO Classes**:

   ```csharp
   public class UpdatedDto
   {
       public int Id { get; set; }
       public string Name { get; set; } = "";
       public DateTime NewProperty { get; set; } // New field
       // Existing properties...
   }
   ```

2. **Update API Responses**:

   - Modify controller methods to return new structure
   - Ensure backward compatibility if possible
   - Update error responses if needed

3. **Update MCP Tool Parsing**:

   - Modify MCP tools to handle new structure
   - Update JSON deserialization
   - Test parsing with new data

4. **Comprehensive Testing**:

   ```powershell
   # Test all layers
   .\test.ps1 -Verbose

   # Run unit tests
   dotnet test FabrikamTests/
   ```

5. **Update Documentation**:
   - Update XML comments
   - Update `api-tests.http` examples
   - Update any schema documentation

### **Testing After Changes**

Always run this testing sequence after making changes:

```powershell
# 1. Quick validation
.\test.ps1 -Quick

# 2. Category-specific testing
.\test.ps1 -ApiOnly    # For API changes
.\test.ps1 -McpOnly    # For MCP changes

# 3. Full integration testing
.\test.ps1 -Verbose

# 4. Unit test validation
dotnet test FabrikamTests/

# 5. Manual verification
# Open api-tests.http and test manually
# Test MCP tools in Copilot
```

### **Quality Checklist**

Before committing changes, ensure:

- [ ] All automated tests pass
- [ ] Manual testing via `api-tests.http` works
- [ ] MCP tools work in Copilot
- [ ] No breaking changes (or properly documented)
- [ ] Error handling is comprehensive
- [ ] Logging is structured and informative
- [ ] Performance impact is acceptable
- [ ] Documentation is updated

---

**Remember**: This is a demonstration platform, so prioritize clarity and educational value while maintaining production-quality code patterns. Always use async/await, proper error handling, and structured logging to showcase modern C# best practices.
