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

    [McpServerTool, Description("Get all orders with optional filtering by status, region, and date range. Returns order details including customer info and items.")]
    public async Task<string> GetOrders(
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
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(region)) queryParams.Add($"region={Uri.EscapeDataString(region)}");
            if (!string.IsNullOrEmpty(fromDate)) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate)}");
            if (!string.IsNullOrEmpty(toDate)) queryParams.Add($"toDate={Uri.EscapeDataString(toDate)}");
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"{baseUrl}/api/orders{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadAsStringAsync();
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                return $"Found {totalCount ?? "unknown"} total orders. Page {page} results:\n{orders}";
            }
            
            return $"Error retrieving orders: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving orders: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get detailed information about a specific order by ID, including customer details and all order items.")]
    public async Task<string> GetOrderById(int orderId)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var response = await _httpClient.GetAsync($"{baseUrl}/api/orders/{orderId}");
            
            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadAsStringAsync();
                return $"Order details for ID {orderId}:\n{order}";
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return $"Order with ID {orderId} not found";
            }
            
            return $"Error retrieving order {orderId}: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving order {orderId}: {ex.Message}";
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

    [McpServerTool, Description("Get all customers with optional filtering by region. Returns customer contact information and summary data.")]
    public async Task<string> GetCustomers(string? region = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(region)) queryParams.Add($"region={Uri.EscapeDataString(region)}");
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/customers{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var customers = await response.Content.ReadAsStringAsync();
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                return $"Found {totalCount ?? "unknown"} total customers. Page {page} results:\n{customers}";
            }
            
            return $"Error retrieving customers: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving customers: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get detailed customer information by ID including order history and support tickets.")]
    public async Task<string> GetCustomerById(int customerId)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var response = await _httpClient.GetAsync($"{baseUrl}/api/customers/{customerId}");
            
            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadAsStringAsync();
                return $"Customer details for ID {customerId}:\n{customer}";
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return $"Customer with ID {customerId} not found";
            }
            
            return $"Error retrieving customer {customerId}: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving customer {customerId}: {ex.Message}";
        }
    }
}
