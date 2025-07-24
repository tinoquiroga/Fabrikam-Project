using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using ModelContextProtocol.Server;
using FabrikamApi.DTOs;

namespace FabrikamMcp.Tools;

[McpServerToolType]
public class FabrikamSalesTools
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public FabrikamSalesTools(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [McpServerTool, Description("Get orders with optional filtering by status, region, date range, or specific order ID. Use orderId for detailed order info, or use filters for order lists. When called without parameters, returns recent orders.")]
    public async Task<object> GetOrders(
        int? orderId = null,
        string? status = null,
        string? region = null,
        string? fromDate = null,
        string? toDate = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            
            // If orderId is provided, get specific order details
            if (orderId.HasValue)
            {
                var orderResponse = await _httpClient.GetAsync($"{baseUrl}/api/orders/{orderId.Value}");
                
                if (orderResponse.IsSuccessStatusCode)
                {
                    var orderJson = await orderResponse.Content.ReadAsStringAsync();
                    var order = JsonSerializer.Deserialize<OrderDetailDto>(orderJson, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    return new
                    {
                        content = new object[]
                        {
                            new
                            {
                                type = "resource",
                                resource = new
                                {
                                    uri = $"{baseUrl}/api/orders/{orderId.Value}",
                                    name = $"Order {order?.OrderNumber ?? orderId.Value.ToString()}",
                                    description = $"Order details for {order?.Customer.Name ?? "customer"}",
                                    mimeType = "application/json"
                                }
                            },
                            new
                            {
                                type = "text",
                                text = FormatOrderDetailText(order)
                            }
                        },
                        orderData = order,
                        outputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                id = new { type = "integer", description = "Order ID" },
                                orderNumber = new { type = "string", description = "Order number" },
                                customer = new { type = "object", description = "Customer information" },
                                status = new { type = "string", description = "Current order status" },
                                financials = new { type = "object", description = "Order financial breakdown" },
                                items = new { type = "array", description = "Order items and products" }
                            }
                        }
                    };
                }
                
                if (orderResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new
                    {
                        error = new
                        {
                            code = 404,
                            message = $"Order with ID {orderId.Value} not found"
                        }
                    };
                }
                
                return new
                {
                    error = new
                    {
                        code = (int)orderResponse.StatusCode,
                        message = $"Error retrieving order {orderId.Value}: {orderResponse.StatusCode} - {orderResponse.ReasonPhrase}"
                    }
                };
            }
            
            // Build query parameters for order list
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(region)) queryParams.Add($"region={Uri.EscapeDataString(region)}");
            if (!string.IsNullOrEmpty(fromDate)) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate)}");
            if (!string.IsNullOrEmpty(toDate)) queryParams.Add($"toDate={Uri.EscapeDataString(toDate)}");
            
            // If no filters provided, default to recent orders (last 30 days) to give meaningful results
            if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(region) && 
                string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
                queryParams.Add($"fromDate={thirtyDaysAgo}");
                fromDate = thirtyDaysAgo; // Set for response message
            }
            
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/orders{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var ordersJson = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<OrderListDto>(ordersJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                return new
                {
                    content = new object[]
                    {
                        new
                        {
                            type = "resource",
                            resource = new
                            {
                                uri = $"{baseUrl}/api/orders{queryString}",
                                name = "Order List",
                                description = GetOrderFilterDescription(status, region, fromDate, toDate),
                                mimeType = "application/json"
                            }
                        },
                        new
                        {
                            type = "text",
                            text = FormatOrderListText(orders, totalCount, page, status, region, fromDate, toDate)
                        }
                    },
                    ordersData = orders,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        totalCount = totalCount
                    },
                    outputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            summary = new { type = "object", description = "Order summary metrics" },
                            byStatus = new { type = "array", description = "Orders grouped by status" },
                            recentOrders = new { type = "array", description = "Recent order details" }
                        }
                    }
                };
            }
            
            return new
            {
                error = new
                {
                    code = (int)response.StatusCode,
                    message = $"Error retrieving orders: {response.StatusCode} - {response.ReasonPhrase}"
                }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                error = new
                {
                    code = 500,
                    message = $"Error retrieving orders: {ex.Message}"
                }
            };
        }
    }

    [McpServerTool, Description("Get sales analytics and summary data including total orders, revenue, average order value, and breakdowns by status and region.")]
    public async Task<object> GetSalesAnalytics(string? fromDate = null, string? toDate = null)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(fromDate)) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate)}");
            if (!string.IsNullOrEmpty(toDate)) queryParams.Add($"toDate={Uri.EscapeDataString(toDate)}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"{baseUrl}/api/orders/analytics{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var analyticsJson = await response.Content.ReadAsStringAsync();
                var analytics = JsonSerializer.Deserialize<SalesAnalyticsDto>(analyticsJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                // Return structured content with both typed data and text fallback for MCP protocol compliance
                var result = new
                {
                    // MCP structured content
                    content = new object[]
                    {
                        new
                        {
                            type = "resource",
                            resource = new
                            {
                                uri = $"{baseUrl}/api/orders/analytics{queryString}",
                                name = "Sales Analytics Data",
                                description = "Comprehensive sales analytics and performance metrics",
                                mimeType = "application/json"
                            }
                        },
                        new
                        {
                            type = "text",
                            text = FormatSalesAnalyticsText(analytics)
                        }
                    },
                    // Structured data for programmatic access
                    analyticsData = analytics,
                    // Schema information for validation
                    outputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            summary = new { type = "object", description = "Overall sales summary metrics" },
                            byStatus = new { type = "array", description = "Sales breakdown by order status" },
                            byRegion = new { type = "array", description = "Sales breakdown by region" },
                            dailyTrends = new { type = "array", description = "Daily sales trend data" }
                        }
                    }
                };
                
                return result;
            }
            
            return new
            {
                error = new
                {
                    code = (int)response.StatusCode,
                    message = $"Error retrieving sales analytics: {response.StatusCode} - {response.ReasonPhrase}"
                }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                error = new
                {
                    code = 500,
                    message = $"Error retrieving sales analytics: {ex.Message}"
                }
            };
        }
    }

    private static string FormatSalesAnalyticsText(SalesAnalyticsDto? analytics)
    {
        if (analytics == null) return "No sales analytics data available.";

        var text = $"""
            📊 SALES ANALYTICS REPORT
            
            📈 Summary ({analytics.Summary.Period.FromDate:yyyy-MM-dd} to {analytics.Summary.Period.ToDate:yyyy-MM-dd})
            • Total Orders: {analytics.Summary.TotalOrders:N0}
            • Total Revenue: ${analytics.Summary.TotalRevenue:N2}
            • Average Order Value: ${analytics.Summary.AverageOrderValue:N2}
            
            📋 By Status:
            """;

        foreach (var status in analytics.ByStatus)
        {
            text += $"\n• {status.Status}: {status.Count:N0} orders (${status.Revenue:N2})";
        }

        text += "\n\n🗺️ By Region:";
        foreach (var region in analytics.ByRegion)
        {
            text += $"\n• {region.Region}: {region.Count:N0} orders (${region.Revenue:N2})";
        }

        if (analytics.DailyTrends.Any())
        {
            text += "\n\n📅 Recent Daily Sales:";
            foreach (var daily in analytics.DailyTrends.TakeLast(7))
            {
                text += $"\n• {daily.Date:MM/dd}: {daily.OrderCount:N0} orders (${daily.Revenue:N2})";
            }
        }

        return text;
    }

    [McpServerTool, Description("Get customers with optional filtering by region or specific customer ID. Use customerId for detailed customer info including order history and support tickets, or use region filter for customer lists. When called without parameters, returns all customers with pagination.")]
    public async Task<object> GetCustomers(int? customerId = null, string? region = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            
            // If customerId is provided, get specific customer details
            if (customerId.HasValue)
            {
                var customerResponse = await _httpClient.GetAsync($"{baseUrl}/api/customers/{customerId.Value}");
                
                if (customerResponse.IsSuccessStatusCode)
                {
                    var customerJson = await customerResponse.Content.ReadAsStringAsync();
                    var customer = JsonSerializer.Deserialize<CustomerDetailDto>(customerJson, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    return new
                    {
                        content = new object[]
                        {
                            new
                            {
                                type = "resource",
                                resource = new
                                {
                                    uri = $"{baseUrl}/api/customers/{customerId.Value}",
                                    name = $"Customer {customer?.FirstName} {customer?.LastName}",
                                    description = "Detailed customer information with order history",
                                    mimeType = "application/json"
                                }
                            },
                            new
                            {
                                type = "text",
                                text = FormatCustomerDetailText(customer)
                            }
                        },
                        customerData = customer,
                        outputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                id = new { type = "integer", description = "Customer ID" },
                                firstName = new { type = "string", description = "Customer first name" },
                                lastName = new { type = "string", description = "Customer last name" },
                                email = new { type = "string", description = "Customer email address" },
                                purchaseHistory = new { type = "object", description = "Customer purchase history summary" }
                            }
                        }
                    };
                }
                
                if (customerResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new
                    {
                        error = new
                        {
                            code = 404,
                            message = $"Customer with ID {customerId.Value} not found"
                        }
                    };
                }
                
                return new
                {
                    error = new
                    {
                        code = (int)customerResponse.StatusCode,
                        message = $"Error retrieving customer {customerId.Value}: {customerResponse.StatusCode} - {customerResponse.ReasonPhrase}"
                    }
                };
            }
            
            // Build query parameters for customer list
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(region)) queryParams.Add($"region={Uri.EscapeDataString(region)}");
            
            // Always include pagination parameters for predictable results
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/customers{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var customersJson = await response.Content.ReadAsStringAsync();
                var customers = JsonSerializer.Deserialize<CustomerInfoDto>(customersJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                return new
                {
                    content = new object[]
                    {
                        new
                        {
                            type = "resource",
                            resource = new
                            {
                                uri = $"{baseUrl}/api/customers{queryString}",
                                name = "Customer Directory",
                                description = $"Customer list{(!string.IsNullOrEmpty(region) ? $" for {region} region" : "")}",
                                mimeType = "application/json"
                            }
                        },
                        new
                        {
                            type = "text",
                            text = FormatCustomerListText(customers, totalCount, page, region)
                        }
                    },
                    customersData = customers,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        totalCount = totalCount
                    },
                    outputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            summary = new { type = "object", description = "Customer base summary statistics" },
                            customers = new { type = "array", description = "Individual customer records" },
                            segments = new { type = "array", description = "Customer segmentation analysis" }
                        }
                    }
                };
            }
            
            return new
            {
                error = new
                {
                    code = (int)response.StatusCode,
                    message = $"Error retrieving customers: {response.StatusCode} - {response.ReasonPhrase}"
                }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                error = new
                {
                    code = 500,
                    message = $"Error retrieving customers: {ex.Message}"
                }
            };
        }
    }

    private static string FormatCustomerDetailText(CustomerDetailDto? customer)
    {
        if (customer == null) return "No customer data available.";

        return $"""
            👤 CUSTOMER PROFILE
            
            📝 Contact Information
            • Name: {customer.FirstName} {customer.LastName}
            • Email: {customer.Email}
            • Phone: {customer.Phone}
            • Status: {customer.Status}
            • Registration Date: {customer.RegistrationDate:yyyy-MM-dd}
            
            📍 Address
            • {customer.Address.Street}
            • {customer.Address.City}, {customer.Address.State} {customer.Address.PostalCode}
            • {customer.Address.Country}
            
            🛒 Purchase History
            • Total Orders: {customer.PurchaseHistory.TotalOrders:N0}
            • Lifetime Value: ${customer.PurchaseHistory.TotalSpent:N2}
            • Average Order: ${customer.PurchaseHistory.AverageOrderValue:N2}
            • First Purchase: {customer.PurchaseHistory.FirstPurchase?.ToString("yyyy-MM-dd") ?? "None"}
            • Last Purchase: {customer.PurchaseHistory.LastPurchase?.ToString("yyyy-MM-dd") ?? "None"}
            
            🏷️ Preferences
            • Contact Method: {customer.Preferences.PreferredContact}
            • Email Opt-in: {(customer.Preferences.EmailOptIn ? "Yes" : "No")}
            • Loyalty Tier: {customer.Preferences.LoyaltyTier}
            """;
    }

    private static string FormatCustomerListText(CustomerInfoDto? customers, string? totalCount, int page, string? region)
    {
        if (customers == null) return "No customer data available.";

        var regionText = !string.IsNullOrEmpty(region) ? $" in {region} region" : "";
        var text = $"""
            👥 CUSTOMER DIRECTORY{regionText.ToUpper()}
            
            📊 Summary
            • Total Customers: {customers.Summary.TotalCustomers:N0}
            • Active Customers: {customers.Summary.ActiveCustomers:N0}
            • New Customers: {customers.Summary.NewCustomers:N0}
            • Retention Rate: {customers.Summary.RetentionRate:P1}
            • Average Lifetime Value: ${customers.Summary.AverageLifetimeValue:N2}
            
            👤 Recent Customers (Page {page}):
            """;

        foreach (var customer in customers.Customers.Take(10))
        {
            text += $"\n• {customer.FirstName} {customer.LastName} ({customer.Email}) - {customer.Status}";
        }

        if (customers.Segments.Any())
        {
            text += "\n\n📈 Customer Segments:";
            foreach (var segment in customers.Segments)
            {
                text += $"\n• {segment.SegmentName}: {segment.CustomerCount:N0} customers ({segment.Percentage:P1})";
            }
        }

        return text;
    }

    private static string FormatOrderDetailText(OrderDetailDto? order)
    {
        if (order == null) return "Order details not available.";

        return $"""
            📋 ORDER DETAILS
            
            🆔 Order Information
            • Order ID: {order.Id}
            • Order Number: {order.OrderNumber}
            • Status: {GetOrderStatusEmoji(order.Status)} {order.Status}
            • Order Date: {order.OrderDate:MMM dd, yyyy}
            {(order.ShippedDate.HasValue ? $"• Shipped: {order.ShippedDate:MMM dd, yyyy}" : "")}
            {(order.DeliveredDate.HasValue ? $"• Delivered: {order.DeliveredDate:MMM dd, yyyy}" : "")}
            
            👤 Customer
            • Name: {order.Customer.Name}
            • Email: {order.Customer.Email}
            {(!string.IsNullOrEmpty(order.Customer.Phone) ? $"• Phone: {order.Customer.Phone}" : "")}
            
            💰 Financial Details
            • Subtotal: ${order.Financials.Subtotal:F2}
            • Tax: ${order.Financials.Tax:F2}
            • Shipping: ${order.Financials.Shipping:F2}
            • Total: ${order.Financials.Total:F2}
            
            📦 Items ({order.Items.Count})
            {string.Join("\n", order.Items.Take(10).Select(item => 
                $"• {item.Product.Name}: {item.Quantity} × ${item.UnitPrice:F2} = ${item.LineTotal:F2}"
            ))}
            {(order.Items.Count > 10 ? $"\n... and {order.Items.Count - 10} more items" : "")}
            
            📍 Shipping Address
            {order.Shipping.FormattedAddress}
            
            {(!string.IsNullOrEmpty(order.Notes) ? $"📝 Notes\n{order.Notes}\n" : "")}
            🕒 Last Updated: {order.LastUpdated:MMM dd, yyyy HH:mm}
            """;
    }

    private static string FormatOrderListText(OrderListDto? orders, string? totalCount, int page, string? status, string? region, string? fromDate, string? toDate)
    {
        if (orders == null) return "No orders found.";

        var text = $"""
            📋 FABRIKAM ORDERS
            
            📊 Summary
            • Total Orders: {totalCount ?? "N/A"}
            • Page: {page}
            • Total Revenue: ${orders.Summary.TotalRevenue:N2}
            • Average Order Value: ${orders.Summary.AverageOrderValue:F2}
            """;

        // Add filter info if applied
        var filters = GetAppliedFilters(status, region, fromDate, toDate);
        if (!string.IsNullOrEmpty(filters))
        {
            text += $"\n\n🔍 Applied Filters:\n{filters}";
        }

        // Show orders by status
        if (orders.ByStatus?.Any() == true)
        {
            text += "\n\n📈 By Status:";
            foreach (var statusGroup in orders.ByStatus.Take(5))
            {
                var emoji = GetOrderStatusEmoji(statusGroup.Status);
                text += $"\n• {emoji} {statusGroup.Status}: {statusGroup.OrderCount} orders (${statusGroup.Revenue:N2})";
            }
        }

        // Show recent orders
        if (orders.RecentOrders?.Any() == true)
        {
            text += "\n\n🕒 Recent Orders:";
            foreach (var order in orders.RecentOrders.Take(8))
            {
                var statusEmoji = GetOrderStatusEmoji(order.Status);
                text += $"\n• #{order.OrderNumber}: {order.Customer.Name} - ${order.Financials.Total:F2} {statusEmoji}";
            }
            if (orders.RecentOrders.Count > 8)
            {
                text += $"\n... and {orders.RecentOrders.Count - 8} more orders";
            }
        }

        // Show regional breakdown if available
        if (orders.Summary.RegionalBreakdown?.Any() == true)
        {
            text += "\n\n🌍 By Region:";
            foreach (var regionalData in orders.Summary.RegionalBreakdown.Take(5))
            {
                text += $"\n• {regionalData.Region}: {regionalData.OrderCount} orders (${regionalData.Revenue:N2})";
            }
        }

        return text;
    }

    private static string GetOrderFilterDescription(string? status, string? region, string? fromDate, string? toDate)
    {
        var filters = new List<string>();
        
        if (!string.IsNullOrEmpty(status)) filters.Add($"Status: {status}");
        if (!string.IsNullOrEmpty(region)) filters.Add($"Region: {region}");
        if (!string.IsNullOrEmpty(fromDate)) filters.Add($"From: {fromDate}");
        if (!string.IsNullOrEmpty(toDate)) filters.Add($"To: {toDate}");

        return filters.Any() ? $"Filtered orders ({string.Join(", ", filters)})" : "Complete order list";
    }

    private static string GetAppliedFilters(string? status, string? region, string? fromDate, string? toDate)
    {
        var filters = new List<string>();
        
        if (!string.IsNullOrEmpty(status)) filters.Add($"• Status: {status}");
        if (!string.IsNullOrEmpty(region)) filters.Add($"• Region: {region}");
        if (!string.IsNullOrEmpty(fromDate)) filters.Add($"• From Date: {fromDate}");
        if (!string.IsNullOrEmpty(toDate)) filters.Add($"• To Date: {toDate}");

        return filters.Any() ? string.Join("\n", filters) : "";
    }

    private static string GetOrderStatusEmoji(string status)
    {
        return status.ToLower() switch
        {
            "pending" => "⏳",
            "confirmed" => "✅",
            "inproduction" => "🏭",
            "in production" => "🏭",
            "readytoship" => "📦",
            "ready to ship" => "📦",
            "shipped" => "🚚",
            "delivered" => "🎉",
            "cancelled" => "❌",
            "onhold" => "⏸️",
            "on hold" => "⏸️",
            _ => "📋"
        };
    }
}
