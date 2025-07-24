using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using ModelContextProtocol.Server;
using FabrikamApi.DTOs;

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
    public async Task<object> GetProducts(
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
                    var productJson = await productResponse.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<ProductDetailDto>(productJson, new JsonSerializerOptions 
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
                                    uri = $"{baseUrl}/api/products/{productId.Value}",
                                    name = product?.Name ?? $"Product {productId.Value}",
                                    description = product?.Description ?? "Product details",
                                    mimeType = "application/json"
                                }
                            },
                            new
                            {
                                type = "text",
                                text = FormatProductDetailText(product)
                            }
                        },
                        productData = product,
                        outputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                id = new { type = "integer", description = "Product ID" },
                                name = new { type = "string", description = "Product name" },
                                category = new { type = "string", description = "Product category" },
                                price = new { type = "number", description = "Product price" },
                                stockQuantity = new { type = "integer", description = "Current stock level" },
                                specifications = new { type = "object", description = "Product specifications and features" }
                            }
                        }
                    };
                }
                
                if (productResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new
                    {
                        error = new
                        {
                            code = 404,
                            message = $"Product with ID {productId.Value} not found"
                        }
                    };
                }
                
                return new
                {
                    error = new
                    {
                        code = (int)productResponse.StatusCode,
                        message = $"Error retrieving product {productId.Value}: {productResponse.StatusCode} - {productResponse.ReasonPhrase}"
                    }
                };
            }
            
            // Handle low stock products request
            if (lowStock)
            {
                var lowStockResponse = await _httpClient.GetAsync($"{baseUrl}/api/products/low-stock");
                
                if (lowStockResponse.IsSuccessStatusCode)
                {
                    var lowStockJson = await lowStockResponse.Content.ReadAsStringAsync();
                    var lowStockProducts = JsonSerializer.Deserialize<List<ProductInventoryDto>>(lowStockJson, new JsonSerializerOptions 
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
                                    uri = $"{baseUrl}/api/products/low-stock",
                                    name = "Low Stock Products",
                                    description = "Products at or below reorder level",
                                    mimeType = "application/json"
                                }
                            },
                            new
                            {
                                type = "text",
                                text = FormatLowStockText(lowStockProducts)
                            }
                        },
                        lowStockProducts = lowStockProducts,
                        outputSchema = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    id = new { type = "integer", description = "Product ID" },
                                    name = new { type = "string", description = "Product name" },
                                    stockQuantity = new { type = "integer", description = "Current stock level" },
                                    reorderLevel = new { type = "integer", description = "Reorder threshold" },
                                    stockStatus = new { type = "string", description = "Stock status indicator" }
                                }
                            }
                        }
                    };
                }
                
                return new
                {
                    error = new
                    {
                        code = (int)lowStockResponse.StatusCode,
                        message = $"Error retrieving low stock products: {lowStockResponse.StatusCode} - {lowStockResponse.ReasonPhrase}"
                    }
                };
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
                var productsJson = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<ProductCatalogDto>(productsJson, new JsonSerializerOptions 
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
                                uri = $"{baseUrl}/api/products{queryString}",
                                name = "Product Catalog",
                                description = GetProductFilterDescription(category, inStock, minPrice, maxPrice),
                                mimeType = "application/json"
                            }
                        },
                        new
                        {
                            type = "text",
                            text = FormatProductCatalogText(products, totalCount, page, category, inStock)
                        }
                    },
                    productsData = products,
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
                            summary = new { type = "object", description = "Product catalog summary" },
                            categories = new { type = "array", description = "Products organized by category" },
                            featuredProducts = new { type = "array", description = "Highlighted products" }
                        }
                    }
                };
            }
            
            return new
            {
                error = new
                {
                    code = (int)response.StatusCode,
                    message = $"Error retrieving products: {response.StatusCode} - {response.ReasonPhrase}"
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
                    message = $"Error retrieving products: {ex.Message}"
                }
            };
        }
    }

    private static string FormatProductDetailText(ProductDetailDto? product)
    {
        if (product == null) return "Product details not available.";

        return $"""
            🏠 PRODUCT DETAILS
            
            📝 Basic Information
            • ID: {product.Id}
            • Name: {product.Name}
            • Category: {product.Category}
            • Price: ${product.Price:F2}
            • Stock: {product.StockQuantity} units
            • Available: {(product.IsAvailable ? "✅ Yes" : "❌ No")}
            
            📋 Description
            {product.Description}
            
            ⚙️ Specifications
            • Material: {product.Specifications?.Material ?? "Not specified"}
            • Dimensions: {product.Specifications?.Dimensions ?? "Not specified"}
            • Weight: {product.Specifications?.Weight ?? "Not specified"}
            {(product.Specifications?.Colors?.Any() == true ? 
                $"• Colors: {string.Join(", ", product.Specifications.Colors)}" : "")}
            {(product.Specifications?.Features?.Any() == true ? 
                $"\n🌟 Features:\n{string.Join("\n", product.Specifications.Features.Select(f => $"  • {f}"))}" : "")}
            
            📊 Stock Status
            • Current Stock: {product.StockQuantity} units
            • Status: {(product.StockQuantity > 0 ? "✅ In Stock" : "❌ Out of Stock")}
            """;
    }

    private static string FormatLowStockText(List<ProductInventoryDto>? products)
    {
        if (products == null || !products.Any()) return "No low stock products found.";

        var text = $"""
            ⚠️ LOW STOCK ALERT
            
            📊 Summary: {products.Count} products need attention
            
            📦 Products at/below reorder level:
            """;

        foreach (var product in products.Take(15))
        {
            var urgency = product.StockQuantity == 0 ? "🔴 OUT OF STOCK" : 
                         product.StockQuantity <= product.ReorderLevel / 2 ? "🟠 CRITICAL" : "🟡 LOW";
            
            text += $"\n• {product.Name}: {product.StockQuantity} units {urgency}";
        }

        if (products.Count > 15)
        {
            text += $"\n\n... and {products.Count - 15} more products";
        }

        return text;
    }

    private static string GetProductFilterDescription(string? category, bool? inStock, decimal? minPrice, decimal? maxPrice)
    {
        var filters = new List<string>();
        
        if (!string.IsNullOrEmpty(category)) filters.Add($"Category: {category}");
        if (inStock.HasValue) filters.Add(inStock.Value ? "In Stock Only" : "Including Out of Stock");
        if (minPrice.HasValue) filters.Add($"Min Price: ${minPrice.Value:F2}");
        if (maxPrice.HasValue) filters.Add($"Max Price: ${maxPrice.Value:F2}");

        return filters.Any() ? $"Filtered product catalog ({string.Join(", ", filters)})" : "Complete product catalog";
    }

    private static string FormatProductCatalogText(ProductCatalogDto? catalog, string? totalCount, int page, string? category, bool? inStock)
    {
        if (catalog == null) return "No products found.";

        var text = $"""
            🏠 FABRIKAM PRODUCT CATALOG
            
            📊 Summary
            • Total Products: {totalCount ?? "N/A"}
            • Page: {page}
            • Categories Available: {catalog.Categories?.Count ?? 0}
            • Featured Products: {catalog.FeaturedProducts?.Count ?? 0}
            """;

        // Add filter info if applied
        if (!string.IsNullOrEmpty(category) || inStock.HasValue)
        {
            text += "\n\n🔍 Applied Filters:";
            if (!string.IsNullOrEmpty(category)) text += $"\n• Category: {category}";
            if (inStock.HasValue) text += $"\n• Stock: {(inStock.Value ? "In Stock Only" : "All Items")}";
        }

        // Show featured products
        if (catalog.FeaturedProducts?.Any() == true)
        {
            text += "\n\n⭐ Featured Products:";
            foreach (var product in catalog.FeaturedProducts.Take(5))
            {
                var stockStatus = product.StockQuantity > 0 ? "✅" : "❌";
                text += $"\n• {product.Name}: ${product.Price:F2} {stockStatus}";
            }
        }

        // Show products by category
        if (catalog.Categories?.Any() == true)
        {
            text += "\n\n📂 By Category:";
            foreach (var categoryGroup in catalog.Categories.Take(5))
            {
                text += $"\n\n🏷️ {categoryGroup.Name} ({categoryGroup.ProductCount} items)";
                if (categoryGroup.Products?.Any() == true)
                {
                    foreach (var product in categoryGroup.Products.Take(3))
                    {
                        var stockIndicator = product.StockQuantity > 0 ? "✅" : "❌";
                        text += $"\n  • {product.Name}: ${product.Price:F2} {stockIndicator}";
                    }
                    if (categoryGroup.Products.Count > 3)
                    {
                        text += $"\n  ... and {categoryGroup.Products.Count - 3} more items";
                    }
                }
            }
        }

        return text;
    }

    [McpServerTool, Description("Get comprehensive inventory operations including summary, stock analysis, and product availability checks. Use summary=true for inventory overview, or provide productIds and quantities to check availability for orders. When called without parameters, returns inventory summary.")]
    public async Task<object> GetInventory(bool summary = true, string? productIds = null, string? quantities = null)
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
                    var inventoryJson = await summaryResponse.Content.ReadAsStringAsync();
                    var inventory = JsonSerializer.Deserialize<InventoryStatusDto>(inventoryJson, new JsonSerializerOptions 
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
                                    uri = $"{baseUrl}/api/products/inventory",
                                    name = "Inventory Status Data",
                                    description = "Current inventory levels and stock analysis",
                                    mimeType = "application/json"
                                }
                            },
                            new
                            {
                                type = "text",
                                text = FormatInventoryText(inventory)
                            }
                        },
                        // Structured data for programmatic access
                        inventoryData = inventory,
                        // Schema information for validation
                        outputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                summary = new { type = "object", description = "Overall inventory summary metrics" },
                                byCategory = new { type = "array", description = "Inventory breakdown by product category" },
                                lowStockAlerts = new { type = "array", description = "Products requiring attention" }
                            }
                        }
                    };
                    
                    return result;
                }
                
                return new
                {
                    error = new
                    {
                        code = (int)summaryResponse.StatusCode,
                        message = $"Error retrieving inventory summary: {summaryResponse.StatusCode} - {summaryResponse.ReasonPhrase}"
                    }
                };
            }
            
            // Handle product availability check
            if (!string.IsNullOrEmpty(productIds) && !string.IsNullOrEmpty(quantities))
            {
                var productIdArray = productIds.Split(',').Select(id => int.Parse(id.Trim())).ToArray();
                var quantityArray = quantities.Split(',').Select(q => int.Parse(q.Trim())).ToArray();

                if (productIdArray.Length != quantityArray.Length)
                {
                    return new
                    {
                        error = new
                        {
                            code = 400,
                            message = "Number of product IDs must match number of quantities"
                        }
                    };
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

                return new
                {
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = $"Product availability check results:\n{string.Join("\n", results)}"
                        }
                    },
                    availabilityResults = results.Select((result, index) => new
                    {
                        productId = productIdArray[index],
                        requestedQuantity = quantityArray[index],
                        result = result
                    }).ToArray()
                };
            }
            
            // Default fallback
            return new
            {
                content = new object[]
                {
                    new
                    {
                        type = "text",
                        text = "Please specify either summary=true for inventory overview, or provide productIds and quantities for availability check."
                    }
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
                    message = $"Error in inventory operation: {ex.Message}"
                }
            };
        }
    }

    private static string FormatInventoryText(InventoryStatusDto? inventory)
    {
        if (inventory == null) return "No inventory data available.";

        var text = $"""
            📦 INVENTORY STATUS REPORT
            
            📊 Summary
            • Total Products: {inventory.Summary.TotalProducts:N0}
            • In Stock: {inventory.Summary.InStockProducts:N0}
            • Low Stock: {inventory.Summary.LowStockProducts:N0}
            • Out of Stock: {inventory.Summary.OutOfStockProducts:N0}
            • Total Inventory Value: ${inventory.Summary.TotalInventoryValue:N2}
            
            📂 By Category:
            """;

        foreach (var category in inventory.ByCategory)
        {
            text += $"\n• {category.Category}: {category.InStock}/{category.TotalProducts} in stock (${category.CategoryValue:N2})";
        }

        if (inventory.LowStockAlerts.Any())
        {
            text += "\n\n⚠️ Low Stock Alerts:";
            foreach (var alert in inventory.LowStockAlerts.Take(10))
            {
                text += $"\n• {alert.Name}: {alert.StockQuantity} units (Reorder at {alert.ReorderLevel})";
            }
        }

        return text;
    }
}
