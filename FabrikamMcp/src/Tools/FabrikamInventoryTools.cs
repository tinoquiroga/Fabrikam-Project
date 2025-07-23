using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace FabrikamMcp.Tools;

[McpServerToolType]
public class FabrikamInventoryTools
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public FabrikamInventoryTools(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [McpServerTool, Description("Get all products with optional filtering by category, stock status, and price range. Returns product details including stock levels.")]
    public async Task<string> GetProducts(
        string? category = null,
        bool? inStock = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");
            if (inStock.HasValue) queryParams.Add($"inStock={inStock.Value.ToString().ToLower()}");
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice.Value}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice.Value}");
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/products{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadAsStringAsync();
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                return $"Found {totalCount ?? "unknown"} total products. Page {page} results:\n{products}";
            }
            
            return $"Error retrieving products: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving products: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get detailed product information by ID including specifications, pricing, and current stock level.")]
    public async Task<string> GetProductById(int productId)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var response = await _httpClient.GetAsync($"{baseUrl}/api/products/{productId}");
            
            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadAsStringAsync();
                return $"Product details for ID {productId}:\n{product}";
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return $"Product with ID {productId} not found";
            }
            
            return $"Error retrieving product {productId}: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving product {productId}: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get comprehensive inventory summary including total products, stock status, low stock items, and inventory value by category.")]
    public async Task<string> GetInventorySummary()
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var response = await _httpClient.GetAsync($"{baseUrl}/api/products/inventory");
            
            if (response.IsSuccessStatusCode)
            {
                var inventory = await response.Content.ReadAsStringAsync();
                return $"Current inventory summary:\n{inventory}";
            }
            
            return $"Error retrieving inventory summary: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving inventory summary: {ex.Message}";
        }
    }

    [McpServerTool, Description("Check if specific products have sufficient stock for an order. Provide comma-separated product IDs and corresponding quantities.")]
    public async Task<string> CheckProductAvailability(string productIds, string quantities)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var productIdArray = productIds.Split(',').Select(id => int.Parse(id.Trim())).ToArray();
            var quantityArray = quantities.Split(',').Select(q => int.Parse(q.Trim())).ToArray();

            if (productIdArray.Length != quantityArray.Length)
            {
                return "Error: Number of product IDs must match number of quantities";
            }

            var results = new List<string>();

            for (int i = 0; i < productIdArray.Length; i++)
            {
                var productId = productIdArray[i];
                var requestedQty = quantityArray[i];
                
                var response = await _httpClient.GetAsync($"{baseUrl}/api/products/{productId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var productJson = await response.Content.ReadFromJsonAsync<dynamic>();
                    var productName = productJson?.GetProperty("name").GetString() ?? "Unknown";
                    var currentStock = productJson?.GetProperty("stockQuantity").GetInt32() ?? 0;
                    var available = currentStock >= requestedQty;
                    
                    results.Add($"Product ID {productId} ({productName}): Requested {requestedQty}, Available {currentStock} - {(available ? "✓ AVAILABLE" : "✗ INSUFFICIENT STOCK")}");
                }
                else
                {
                    results.Add($"Product ID {productId}: Not found");
                }
            }

            return $"Stock availability check results:\n{string.Join("\n", results)}";
        }
        catch (Exception ex)
        {
            return $"Error checking product availability: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get products that are currently out of stock or running low (at or below reorder level).")]
    public async Task<string> GetLowStockProducts()
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            var response = await _httpClient.GetAsync($"{baseUrl}/api/products/inventory");
            
            if (response.IsSuccessStatusCode)
            {
                var inventoryJson = await response.Content.ReadFromJsonAsync<dynamic>();
                var lowStockItems = inventoryJson?.GetProperty("lowStockItems");
                var outOfStockCount = inventoryJson?.GetProperty("outOfStockProducts").GetInt32() ?? 0;
                
                return $"Inventory Status Alert:\n" +
                       $"Products out of stock: {outOfStockCount}\n" +
                       $"Low stock items requiring attention:\n{lowStockItems}";
            }
            
            return $"Error retrieving low stock information: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving low stock information: {ex.Message}";
        }
    }
}
