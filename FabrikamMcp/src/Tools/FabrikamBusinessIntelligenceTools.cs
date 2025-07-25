using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace FabrikamMcp.Tools;

[McpServerToolType]
public class FabrikamBusinessIntelligenceTools
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public FabrikamBusinessIntelligenceTools(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [McpServerTool, Description("Get comprehensive business dashboard with key metrics across sales, inventory, and customer service. Provides executive-level insights and performance indicators.")]
    public async Task<object> GetBusinessDashboard(
        string? timeframe = "30days",
        bool includeForecasts = false)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7241";
            
            // Calculate date range based on timeframe
            var (fromDate, toDate) = GetDateRange(timeframe);
            
            // Fetch data from multiple APIs concurrently
            var salesTask = _httpClient.GetAsync($"{baseUrl}/api/orders/analytics?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
            var supportTask = _httpClient.GetAsync($"{baseUrl}/api/supporttickets/analytics?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
            var productsTask = _httpClient.GetAsync($"{baseUrl}/api/products?pageSize=1000");
            var ordersTask = _httpClient.GetAsync($"{baseUrl}/api/orders?pageSize=100");
            var ticketsTask = _httpClient.GetAsync($"{baseUrl}/api/supporttickets?pageSize=100");
            
            await Task.WhenAll(salesTask, supportTask, productsTask, ordersTask, ticketsTask);
            
            var dashboardText = "📊 **Fabrikam Business Dashboard**\n\n";
            dashboardText += $"📅 **Period:** {timeframe} ({fromDate:MMM dd} - {toDate:MMM dd, yyyy})\n\n";
            
            // Sales Performance
            if (salesTask.Result.IsSuccessStatusCode)
            {
                var salesJson = await salesTask.Result.Content.ReadAsStringAsync();
                using var salesDoc = JsonDocument.Parse(salesJson);
                var salesData = salesDoc.RootElement;
                
                dashboardText += "## 💰 **Sales Performance**\n";
                
                if (salesData.TryGetProperty("summary", out var summary))
                {
                    var totalOrders = GetJsonValue(summary, "totalOrders");
                    var totalRevenue = GetJsonValue(summary, "totalRevenue");
                    var averageOrderValue = GetJsonValue(summary, "averageOrderValue");
                    var pendingOrders = GetJsonValue(summary, "pendingOrders");
                    
                    dashboardText += $"- **Total Orders:** {totalOrders}\n";
                    dashboardText += $"- **Total Revenue:** ${decimal.Parse(totalRevenue):N2}\n";
                    dashboardText += $"- **Average Order Value:** ${decimal.Parse(averageOrderValue):N2}\n";
                    dashboardText += $"- **Pending Orders:** {pendingOrders}\n";
                }
                
                if (salesData.TryGetProperty("byRegion", out var regions))
                {
                    dashboardText += "\n**Top Performing Regions:**\n";
                    foreach (var region in regions.EnumerateArray().Take(3))
                    {
                        var regionName = GetJsonValue(region, "region");
                        var revenue = GetJsonValue(region, "totalRevenue");
                        var orders = GetJsonValue(region, "orderCount");
                        dashboardText += $"• {regionName}: ${decimal.Parse(revenue):N0} ({orders} orders)\n";
                    }
                }
            }
            
            // Customer Service Health
            if (supportTask.Result.IsSuccessStatusCode)
            {
                var supportJson = await supportTask.Result.Content.ReadAsStringAsync();
                using var supportDoc = JsonDocument.Parse(supportJson);
                var supportData = supportDoc.RootElement;
                
                dashboardText += "\n## 🎧 **Customer Service Health**\n";
                
                if (supportData.TryGetProperty("summary", out var supportSummary))
                {
                    var totalTickets = GetJsonValue(supportSummary, "totalTickets");
                    var openTickets = GetJsonValue(supportSummary, "openTickets");
                    var resolvedTickets = GetJsonValue(supportSummary, "resolvedTickets");
                    var avgResolutionDays = GetJsonValue(supportSummary, "averageResolutionDays");
                    
                    dashboardText += $"- **Total Tickets:** {totalTickets}\n";
                    dashboardText += $"- **Open Tickets:** {openTickets}\n";
                    dashboardText += $"- **Resolved Tickets:** {resolvedTickets}\n";
                    dashboardText += $"- **Avg Resolution Time:** {decimal.Parse(avgResolutionDays):F1} days\n";
                    
                    // Calculate resolution rate
                    var totalInt = int.Parse(totalTickets);
                    var resolvedInt = int.Parse(resolvedTickets);
                    if (totalInt > 0)
                    {
                        var resolutionRate = (double)resolvedInt / totalInt * 100;
                        dashboardText += $"- **Resolution Rate:** {resolutionRate:F1}%\n";
                    }
                }
                
                if (supportData.TryGetProperty("byPriority", out var priorities))
                {
                    dashboardText += "\n**Tickets by Priority:**\n";
                    foreach (var priority in priorities.EnumerateArray())
                    {
                        var priorityName = GetJsonValue(priority, "priority");
                        var count = GetJsonValue(priority, "count");
                        var percentage = GetJsonValue(priority, "percentage");
                        var icon = priorityName switch
                        {
                            "Critical" => "🚨",
                            "High" => "🔴",
                            "Medium" => "🟡",
                            "Low" => "🟢",
                            _ => "📋"
                        };
                        dashboardText += $"• {icon} {priorityName}: {count} ({percentage}%)\n";
                    }
                }
            }
            
            // Inventory Status
            if (productsTask.Result.IsSuccessStatusCode)
            {
                var productsJson = await productsTask.Result.Content.ReadAsStringAsync();
                using var productsDoc = JsonDocument.Parse(productsJson);
                var productsArray = productsDoc.RootElement;
                
                var totalProducts = 0;
                var inStock = 0;
                var lowStock = 0;
                var outOfStock = 0;
                var totalInventoryValue = 0m;
                
                foreach (var product in productsArray.EnumerateArray())
                {
                    totalProducts++;
                    var stockStatus = GetJsonValue(product, "stockStatus");
                    var price = decimal.Parse(GetJsonValue(product, "price", "0"));
                    var quantity = int.Parse(GetJsonValue(product, "stockQuantity", "0"));
                    
                    totalInventoryValue += price * quantity;
                    
                    switch (stockStatus)
                    {
                        case "In Stock": inStock++; break;
                        case "Low Stock": lowStock++; break;
                        case "Out of Stock": outOfStock++; break;
                    }
                }
                
                dashboardText += "\n## 📦 **Inventory Status**\n";
                dashboardText += $"- **Total Products:** {totalProducts}\n";
                dashboardText += $"- **In Stock:** {inStock} ({(double)inStock / totalProducts * 100:F1}%)\n";
                dashboardText += $"- **Low Stock:** {lowStock} ({(double)lowStock / totalProducts * 100:F1}%)\n";
                dashboardText += $"- **Out of Stock:** {outOfStock} ({(double)outOfStock / totalProducts * 100:F1}%)\n";
                dashboardText += $"- **Total Inventory Value:** ${totalInventoryValue:N2}\n";
                
                // Inventory health score
                var healthScore = ((double)(inStock + lowStock * 0.5) / totalProducts * 100);
                var healthIcon = healthScore switch
                {
                    >= 90 => "🟢",
                    >= 70 => "🟡",
                    _ => "🔴"
                };
                dashboardText += $"- **Inventory Health:** {healthIcon} {healthScore:F1}%\n";
            }
            
            // Recent Activity Summary
            dashboardText += "\n## 📈 **Recent Activity**\n";
            
            // Recent orders
            if (ordersTask.Result.IsSuccessStatusCode)
            {
                var ordersJson = await ordersTask.Result.Content.ReadAsStringAsync();
                using var ordersDoc = JsonDocument.Parse(ordersJson);
                var ordersArray = ordersDoc.RootElement;
                
                var recentOrders = 0;
                var pendingOrders = 0;
                foreach (var order in ordersArray.EnumerateArray().Take(10))
                {
                    recentOrders++;
                    var status = GetJsonValue(order, "status");
                    if (status == "Pending" || status == "Processing") pendingOrders++;
                }
                
                dashboardText += $"- **Recent Orders:** {recentOrders} in last 100 transactions\n";
                dashboardText += $"- **Orders Needing Attention:** {pendingOrders}\n";
            }
            
            // Recent tickets
            if (ticketsTask.Result.IsSuccessStatusCode)
            {
                var ticketsJson = await ticketsTask.Result.Content.ReadAsStringAsync();
                using var ticketsDoc = JsonDocument.Parse(ticketsJson);
                var ticketsArray = ticketsDoc.RootElement;
                
                var recentTickets = 0;
                var urgentTickets = 0;
                foreach (var ticket in ticketsArray.EnumerateArray().Take(10))
                {
                    recentTickets++;
                    var priority = GetJsonValue(ticket, "priority");
                    if (priority == "High" || priority == "Critical") urgentTickets++;
                }
                
                dashboardText += $"- **Recent Support Tickets:** {recentTickets} in last 100 tickets\n";
                dashboardText += $"- **Urgent Tickets:** {urgentTickets}\n";
            }
            
            // Action Items
            dashboardText += "\n## 🎯 **Action Items**\n";
            var actionItems = new List<string>();
            
            // Check for urgent issues
            if (outOfStock > 0) actionItems.Add($"🔴 {outOfStock} products are out of stock");
            if (lowStock > 5) actionItems.Add($"🟡 {lowStock} products are low in stock");
            
            if (actionItems.Any())
            {
                foreach (var item in actionItems)
                {
                    dashboardText += $"- {item}\n";
                }
            }
            else
            {
                dashboardText += "✅ No immediate action items - operations running smoothly\n";
            }
            
            // Add forecasting if requested
            if (includeForecasts)
            {
                dashboardText += "\n## 🔮 **Forecasts & Trends**\n";
                dashboardText += "- Sales trend analysis and predictions\n";
                dashboardText += "- Inventory restocking recommendations\n";
                dashboardText += "- Customer service capacity planning\n";
                dashboardText += "*Note: Advanced forecasting features coming soon*\n";
            }
            
            return new
            {
                content = new object[]
                {
                    new { type = "text", text = dashboardText }
                },
                data = new
                {
                    timeframe,
                    fromDate,
                    toDate,
                    metrics = new
                    {
                        inventoryHealth = (double)(inStock + lowStock * 0.5) / (inStock + lowStock + outOfStock) * 100,
                        actionItemsCount = actionItems.Count
                    }
                }
            };
        }
        catch (HttpRequestException ex)
        {
            return new
            {
                error = new { message = $"Network error retrieving business dashboard: {ex.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                error = new { message = $"Error retrieving business dashboard: {ex.Message}" }
            };
        }
    }

    [McpServerTool, Description("Get performance alerts and recommendations for business operations. Identifies issues requiring immediate attention and provides actionable insights.")]
    public async Task<object> GetBusinessAlerts(
        string? severity = "all",
        bool includeRecommendations = true)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7241";
            
            // Fetch current data to analyze for alerts
            var productsTask = _httpClient.GetAsync($"{baseUrl}/api/products?pageSize=1000");
            var ordersTask = _httpClient.GetAsync($"{baseUrl}/api/orders?status=Pending&pageSize=100");
            var urgentTicketsTask = _httpClient.GetAsync($"{baseUrl}/api/supporttickets?urgent=true&pageSize=100");
            
            await Task.WhenAll(productsTask, ordersTask, urgentTicketsTask);
            
            var alerts = new List<(string severity, string title, string description, string action)>();
            
            // Inventory alerts
            if (productsTask.Result.IsSuccessStatusCode)
            {
                var productsJson = await productsTask.Result.Content.ReadAsStringAsync();
                using var productsDoc = JsonDocument.Parse(productsJson);
                var productsArray = productsDoc.RootElement;
                
                var outOfStockProducts = new List<string>();
                var lowStockProducts = new List<string>();
                
                foreach (var product in productsArray.EnumerateArray())
                {
                    var name = GetJsonValue(product, "name");
                    var stockStatus = GetJsonValue(product, "stockStatus");
                    var stockQuantity = int.Parse(GetJsonValue(product, "stockQuantity", "0"));
                    
                    if (stockStatus == "Out of Stock")
                    {
                        outOfStockProducts.Add(name);
                    }
                    else if (stockStatus == "Low Stock")
                    {
                        lowStockProducts.Add($"{name} ({stockQuantity} units)");
                    }
                }
                
                if (outOfStockProducts.Any())
                {
                    alerts.Add(("critical", 
                        $"🚨 {outOfStockProducts.Count} Products Out of Stock",
                        $"Products unavailable: {string.Join(", ", outOfStockProducts.Take(3))}{(outOfStockProducts.Count > 3 ? $" and {outOfStockProducts.Count - 3} more" : "")}",
                        "Immediate restocking required to avoid lost sales"));
                }
                
                if (lowStockProducts.Count > 5)
                {
                    alerts.Add(("high",
                        $"⚠️ {lowStockProducts.Count} Products Low in Stock",
                        $"Low inventory: {string.Join(", ", lowStockProducts.Take(3))}{(lowStockProducts.Count > 3 ? $" and {lowStockProducts.Count - 3} more" : "")}",
                        "Review inventory levels and place reorders"));
                }
            }
            
            // Order processing alerts
            if (ordersTask.Result.IsSuccessStatusCode)
            {
                var ordersJson = await ordersTask.Result.Content.ReadAsStringAsync();
                using var ordersDoc = JsonDocument.Parse(ordersJson);
                var ordersArray = ordersDoc.RootElement;
                
                var pendingOrders = ordersArray.GetArrayLength();
                var oldPendingOrders = 0;
                
                foreach (var order in ordersArray.EnumerateArray())
                {
                    var orderDate = DateTime.Parse(GetJsonValue(order, "orderDate"));
                    if (DateTime.UtcNow - orderDate > TimeSpan.FromDays(3))
                    {
                        oldPendingOrders++;
                    }
                }
                
                if (pendingOrders > 20)
                {
                    alerts.Add(("high",
                        $"📋 High Pending Order Volume",
                        $"{pendingOrders} orders awaiting processing",
                        "Review order processing capacity and workflow efficiency"));
                }
                
                if (oldPendingOrders > 0)
                {
                    alerts.Add(("critical",
                        $"⏰ {oldPendingOrders} Orders Overdue",
                        $"Orders pending for more than 3 days",
                        "Immediate attention required to prevent customer dissatisfaction"));
                }
            }
            
            // Customer service alerts
            if (urgentTicketsTask.Result.IsSuccessStatusCode)
            {
                var ticketsJson = await urgentTicketsTask.Result.Content.ReadAsStringAsync();
                using var ticketsDoc = JsonDocument.Parse(ticketsJson);
                var ticketsArray = ticketsDoc.RootElement;
                
                var urgentTickets = ticketsArray.GetArrayLength();
                var criticalTickets = 0;
                
                foreach (var ticket in ticketsArray.EnumerateArray())
                {
                    var priority = GetJsonValue(ticket, "priority");
                    if (priority == "Critical") criticalTickets++;
                }
                
                if (criticalTickets > 0)
                {
                    alerts.Add(("critical",
                        $"🆘 {criticalTickets} Critical Support Tickets",
                        $"High-priority customer issues requiring immediate response",
                        "Escalate to senior support staff immediately"));
                }
                
                if (urgentTickets > 10)
                {
                    alerts.Add(("high",
                        $"🎧 High Support Ticket Volume",
                        $"{urgentTickets} high/critical priority tickets active",
                        "Consider additional support resources or process optimization"));
                }
            }
            
            // Filter by severity if specified
            if (severity != "all")
            {
                alerts = alerts.Where(a => a.severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            var alertsText = "🚨 **Business Alerts & Recommendations**\n\n";
            
            if (!alerts.Any())
            {
                alertsText += "✅ **No active alerts** - All systems operating within normal parameters\n";
                alertsText += "\n💡 **Tip:** Regular monitoring helps maintain operational excellence\n";
            }
            else
            {
                var criticalCount = alerts.Count(a => a.severity == "critical");
                var highCount = alerts.Count(a => a.severity == "high");
                var mediumCount = alerts.Count(a => a.severity == "medium");
                
                alertsText += $"📊 **Alert Summary:** {criticalCount} Critical | {highCount} High | {mediumCount} Medium\n\n";
                
                foreach (var (alertSeverity, title, description, action) in alerts.OrderBy(a => a.severity == "critical" ? 0 : a.severity == "high" ? 1 : 2))
                {
                    alertsText += $"### {title}\n";
                    alertsText += $"**Description:** {description}\n";
                    
                    if (includeRecommendations)
                    {
                        alertsText += $"**Recommended Action:** {action}\n";
                    }
                    
                    alertsText += "\n";
                }
                
                if (includeRecommendations)
                {
                    alertsText += "## 💡 **General Recommendations**\n";
                    alertsText += "- Set up automated inventory reorder points\n";
                    alertsText += "- Implement order processing SLA monitoring\n";
                    alertsText += "- Create escalation procedures for critical support tickets\n";
                    alertsText += "- Regular review of operational metrics and thresholds\n";
                }
            }
            
            return new
            {
                content = new object[]
                {
                    new { type = "text", text = alertsText }
                },
                data = new
                {
                    alertCount = alerts.Count,
                    alertsBySeverity = new
                    {
                        critical = alerts.Count(a => a.severity == "critical"),
                        high = alerts.Count(a => a.severity == "high"),
                        medium = alerts.Count(a => a.severity == "medium")
                    },
                    alerts = alerts.Select(a => new
                    {
                        a.severity,
                        a.title,
                        a.description,
                        a.action
                    })
                }
            };
        }
        catch (HttpRequestException ex)
        {
            return new
            {
                error = new { message = $"Network error retrieving business alerts: {ex.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                error = new { message = $"Error retrieving business alerts: {ex.Message}" }
            };
        }
    }

    private static (DateTime fromDate, DateTime toDate) GetDateRange(string? timeframe)
    {
        var toDate = DateTime.UtcNow;
        var fromDate = timeframe?.ToLower() switch
        {
            "7days" or "week" => toDate.AddDays(-7),
            "30days" or "month" => toDate.AddDays(-30),
            "90days" or "quarter" => toDate.AddDays(-90),
            "365days" or "year" => toDate.AddDays(-365),
            _ => toDate.AddDays(-30) // Default to 30 days
        };
        
        return (fromDate, toDate);
    }

    private static string GetJsonValue(JsonElement element, string propertyName, string defaultValue = "")
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString() ?? defaultValue,
                JsonValueKind.Number => property.TryGetDecimal(out var dec) ? dec.ToString("F2") : property.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => defaultValue,
                _ => property.GetRawText()
            };
        }
        return defaultValue;
    }
}
