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

    [McpServerTool, Description("Get products with optional filtering by category, stock status, price range, specific product ID, or low stock items. Use productId for detailed product info, lowStock=true for items at/below reorder level, or use filters for product lists. When called without parameters, returns all available products.")]
    public async Task<string> GetProducts(
        int? productId = null,
        string? category = null,
        bool? inStock = null,
        bool lowStock = false,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            
            // If productId is provided, get specific product details
            if (productId.HasValue)
            {
                var productResponse = await _httpClient.GetAsync($"{baseUrl}/api/products/{productId.Value}");
                
                if (productResponse.IsSuccessStatusCode)
                {
                    var product = await productResponse.Content.ReadAsStringAsync();
                    return $"Product details for ID {productId.Value}:\n{product}";
                }
                
                if (productResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return $"Product with ID {productId.Value} not found";
                }
                
                return $"Error retrieving product {productId.Value}: {productResponse.StatusCode} - {productResponse.ReasonPhrase}";
            }
            
            // Handle low stock products request
            if (lowStock)
            {
                var lowStockResponse = await _httpClient.GetAsync($"{baseUrl}/api/products/low-stock");
                
                if (lowStockResponse.IsSuccessStatusCode)
                {
                    var lowStockProducts = await lowStockResponse.Content.ReadAsStringAsync();
                    return $"Products that are out of stock or running low (at or below reorder level):\n{lowStockProducts}";
                }
                
                return $"Error retrieving low stock products: {lowStockResponse.StatusCode} - {lowStockResponse.ReasonPhrase}";
            }
            
            // Build query parameters for product list
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");
            if (inStock.HasValue) queryParams.Add($"inStock={inStock.Value.ToString().ToLower()}");
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice.Value}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice.Value}");
            
            // Always include pagination for predictable results
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/products{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadAsStringAsync();
                var totalCount = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                
                var filterText = "";
                if (!string.IsNullOrEmpty(category)) filterText += $" in {category} category";
                if (inStock.HasValue) filterText += inStock.Value ? " (in stock)" : " (including out of stock)";
                
                return $"Found {totalCount ?? "unknown"} total products{filterText}. Page {page} results:\n{products}";
            }
            
            return $"Error retrieving products: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving products: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get comprehensive inventory operations including summary, stock analysis, and product availability checks. Use summary=true for inventory overview, or provide productIds and quantities to check availability for orders. When called without parameters, returns inventory summary.")]
    public async Task<string> GetInventory(bool summary = true, string? productIds = null, string? quantities = null)
    {
        try
        {
            var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
            
            // Handle inventory summary request (default behavior)
            if (summary || (string.IsNullOrEmpty(productIds) && string.IsNullOrEmpty(quantities)))
            {
                var summaryResponse = await _httpClient.GetAsync($"{baseUrl}/api/products/inventory");
                
                if (summaryResponse.IsSuccessStatusCode)
                {
                    var inventory = await summaryResponse.Content.ReadAsStringAsync();
                    return $"Current inventory summary:\n{inventory}";
                }
                
                return $"Error retrieving inventory summary: {summaryResponse.StatusCode} - {summaryResponse.ReasonPhrase}";
            }
            
            // Handle product availability check
            if (!string.IsNullOrEmpty(productIds) && !string.IsNullOrEmpty(quantities))
            {
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

                return $"Product availability check results:\n{string.Join("\n", results)}";
            }
            
            // Default fallback
            return "Please specify either summary=true for inventory overview, or provide productIds and quantities for availability check.";
        }
        catch (Exception ex)
        {
            return $"Error in inventory operation: {ex.Message}";
        }
    }
}
