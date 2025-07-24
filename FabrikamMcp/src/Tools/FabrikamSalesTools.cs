using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

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
    public async Task<string> GetOrders(
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
                    var order = await orderResponse.Content.ReadAsStringAsync();
                    return $"Order details for ID {orderId.Value}:\n{order}";
                }
                
                if (orderResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return $"Order with ID {orderId.Value} not found";
                }
                
                return $"Error retrieving order {orderId.Value}: {orderResponse.StatusCode} - {orderResponse.ReasonPhrase}";
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
                var orders = await response.Content.ReadAsStringAsync();
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                var contextMessage = "";
                if (!string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(status) && string.IsNullOrEmpty(region))
                {
                    contextMessage = " (showing recent orders from last 30 days)";
                }
                
                return $"Found {totalCount ?? "unknown"} total orders{contextMessage}. Page {page} results:\n{orders}";
            }
            
            return $"Error retrieving orders: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving orders: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get sales analytics and summary data including total orders, revenue, average order value, and breakdowns by status and region.")]
    public async Task<string> GetSalesAnalytics(string? fromDate = null, string? toDate = null)
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
                var analytics = await response.Content.ReadAsStringAsync();
                return $"Sales analytics:\n{analytics}";
            }
            
            return $"Error retrieving sales analytics: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving sales analytics: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get customers with optional filtering by region or specific customer ID. Use customerId for detailed customer info including order history and support tickets, or use region filter for customer lists. When called without parameters, returns all customers with pagination.")]
    public async Task<string> GetCustomers(int? customerId = null, string? region = null, int page = 1, int pageSize = 20)
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
                    var customer = await customerResponse.Content.ReadAsStringAsync();
                    return $"Customer details for ID {customerId.Value}:\n{customer}";
                }
                
                if (customerResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return $"Customer with ID {customerId.Value} not found";
                }
                
                return $"Error retrieving customer {customerId.Value}: {customerResponse.StatusCode} - {customerResponse.ReasonPhrase}";
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
                var customers = await response.Content.ReadAsStringAsync();
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                var regionText = !string.IsNullOrEmpty(region) ? $" in {region} region" : "";
                return $"Found {totalCount ?? "unknown"} total customers{regionText}. Page {page} results:\n{customers}";
            }
            
            return $"Error retrieving customers: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving customers: {ex.Message}";
        }
    }
}
