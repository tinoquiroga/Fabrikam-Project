# MCP Tools Architecture - Fabrikam Business Platform

## 🏗️ **Architecture Overview**

The Fabrikam MCP (Model Context Protocol) server provides AI-powered business tools through a standardized protocol, enabling natural language interaction with business operations.

### **Server Configuration**

- **Host**: `localhost:5000` (HTTP) / `localhost:5001` (HTTPS)
- **Protocol**: MCP 1.0 Standard
- **Platform**: .NET 9.0 / ASP.NET Core
- **Location**: `FabrikamMcp/src/FabrikamMcp.csproj`

## 🧩 **Tool Class Architecture**

### **1. FabrikamSalesTools** (3 tools)

**Business Focus**: Sales pipeline and performance analytics

- `GetSalesAnalytics` - Revenue trends and performance metrics
- `GetSalesTeamPerformance` - Individual and team sales tracking  
- `GetQuarterlyReport` - Quarterly business summaries

### **2. FabrikamProductTools** (2 tools)

**Business Focus**: Product catalog and inventory management

- `GetProducts` - Product catalog with filtering capabilities
- `GetProductAnalytics` - Product performance and trends

### **3. FabrikamInventoryTools** (1 tool)

**Business Focus**: Stock and inventory tracking

- `GetInventoryReport` - Current stock levels and availability

### **4. FabrikamCustomerServiceTools** (4 tools)

**Business Focus**: Customer relationship and order management

- `GetCustomers` - Customer directory and information
- `GetCustomerAnalytics` - Customer behavior insights
- `GetOrders` - Order tracking and management
- `GetOrderTimeline` - Detailed order progression tracking

### **5. FabrikamBusinessIntelligenceTools** (2 tools)

**Business Focus**: Executive insights and business intelligence

- `GetBusinessDashboard` - High-level business metrics
- `GetRevenueAnalytics` - Revenue analysis and forecasting

### **HTTP Client Pattern**

All tools follow a consistent HTTP client pattern:

```csharp
[McpServerTool, Description("Tool description")]
public async Task<object> ToolName(string? parameter = null)
{
    try
    {
        var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
        var response = await _httpClient.GetAsync($"{baseUrl}/api/endpoint");
        
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
        
        return new { error = new { message = $"Error: {response.StatusCode}" } };
    }
    catch (Exception ex)
    {
        return new { error = new { message = $"Error: {ex.Message}" } };
    }
}
```

### **Response Format**

- **Success**: Structured object with `content` array and raw `data`
- **Error**: Structured object with `error.message` for graceful failure handling
- **Content Type**: Text-based responses optimized for AI consumption

### **API Integration**

- **Base URL**: Configurable via `FabrikamApi:BaseUrl` setting
- **Default**: `https://localhost:7297` (FabrikamApi server)
- **Authentication**: Inherits from HTTP client configuration
- **Error Handling**: Comprehensive exception management with user-friendly messages

## 🧪 **Testing Architecture**

### **Test Coverage**

- **Unit Tests**: 11/11 MCP tools passing (100% coverage)
- **Integration Tests**: API-MCP interaction validation
- **Protocol Tests**: MCP standard compliance verification
- **Performance Tests**: Tool response time monitoring

### **Testing Framework**

Located in `scripts/testing/Test-Mcp.ps1`:

- Tool discovery and enumeration
- Individual tool execution validation
- Error scenario testing
- Performance benchmarking
- Protocol compliance verification

### **Test Categories**

1. **Functional Testing**: Each tool returns expected data structure
2. **Error Handling**: Graceful failure under various conditions
3. **Performance**: Response time within acceptable thresholds
4. **Integration**: Tools work correctly with API changes
5. **Protocol**: MCP standard compliance verification

## 🔄 **Development Workflow Integration**

### **VS Code Tasks**

- `🤖 Start MCP Server` - Individual MCP server startup
- `🌐 Start Both Servers` - Full stack with API + MCP
- `📊 Project Status` - Health check including MCP tools

### **Testing Commands**

```powershell
# Quick MCP validation
.\test.ps1 -McpOnly

# Comprehensive MCP testing  
.\scripts\testing\Test-Mcp.ps1 -Verbose

# Full integration testing
.\test.ps1 -Verbose
```

## 📈 **Business Value Delivery**

### **AI-Powered Business Operations**

- **Natural Language**: Query business data using plain English
- **Comprehensive Coverage**: Sales, products, inventory, customers, and BI
- **Real-Time Data**: Direct API integration for current business state
- **Scalable Architecture**: Modular tool classes for easy expansion

### **Use Case Examples**

- "Show me this quarter's sales performance by region"
- "What products are low in stock and need reordering?"
- "Give me a customer analytics summary for the northeast region"
- "Display the business dashboard with key metrics"

## 🔮 **Future Enhancements**

### **Planned Extensions**

- **Write Operations**: Order creation, customer updates, inventory adjustments
- **Advanced Analytics**: Predictive modeling, trend forecasting
- **Multi-Modal**: Image/chart generation for visual insights
- **Real-Time Events**: WebSocket integration for live data streams

### **Protocol Evolution**

- **Enhanced Error Handling**: More granular error categorization
- **Batch Operations**: Multiple tool execution in single request
- **Caching Strategy**: Intelligent response caching for performance
- **Authentication**: Advanced security for production deployment

---

**Documentation Status**: Current as of Issue #12 completion (January 2025)

**Related Documents**:

- `TESTING-STRATEGY.md` - Comprehensive testing methodology
- `archive/completed-tasks/MCP-TOOLS-ISSUE-12-COMPLETE.md` - Detailed implementation history
- `TODO-FUTURE-ENHANCEMENTS.md` - Planned feature roadmap
