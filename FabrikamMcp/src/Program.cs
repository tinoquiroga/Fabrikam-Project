using FabrikamMcp.Tools;
using ModelContextProtocol;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add MCP server services with HTTP transport and Fabrikam business tools
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<FabrikamSalesTools>()
    .WithTools<FabrikamInventoryTools>()
    .WithTools<FabrikamCustomerServiceTools>()
    .WithTools<FabrikamProductTools>()
    .WithTools<FabrikamBusinessIntelligenceTools>();

// Add CORS for HTTP transport support in browsers
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable CORS
app.UseCors();

// Map MCP endpoints to the standard /mcp path
app.MapMcp("/mcp");

// Add status and info endpoints
app.MapGet("/status", () => new
{
    Status = "Ready",
    Service = "Fabrikam MCP Server",
    Version = "1.0.0",
    Description = "Model Context Protocol server for Fabrikam Modular Homes business operations",
    Transport = "HTTP",
    BusinessModules = new[]
    {
        "Sales - Order management and customer analytics",
        "Inventory - Product catalog and stock monitoring", 
        "Customer Service - Support ticket management and resolution",
        "Products - Product catalog, inventory analytics and management",
        "Business Intelligence - Executive dashboards and performance alerts"
    },
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
});

// Redirect root path to status for convenience
app.MapGet("/", () => Results.Redirect("/status"));

app.Run();
