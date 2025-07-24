using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.DTOs;

/// <summary>
/// Comprehensive inventory status and analytics
/// </summary>
public class InventoryStatusDto
{
    /// <summary>
    /// High-level inventory summary
    /// </summary>
    public InventorySummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Inventory breakdown by product category
    /// </summary>
    public List<InventoryByCategoryDto> ByCategory { get; set; } = new();
    
    /// <summary>
    /// Products that need attention (low stock or out of stock)
    /// </summary>
    public List<ProductInventoryDto> LowStockAlerts { get; set; } = new();
}

/// <summary>
/// Overall inventory summary metrics
/// </summary>
public class InventorySummaryDto
{
    /// <summary>
    /// Total number of unique products in catalog
    /// </summary>
    public int TotalProducts { get; set; }
    
    /// <summary>
    /// Number of products currently in stock
    /// </summary>
    public int InStockProducts { get; set; }
    
    /// <summary>
    /// Number of products with low stock levels
    /// </summary>
    public int LowStockProducts { get; set; }
    
    /// <summary>
    /// Number of products that are out of stock
    /// </summary>
    public int OutOfStockProducts { get; set; }
    
    /// <summary>
    /// Total estimated value of current inventory
    /// </summary>
    public decimal TotalInventoryValue { get; set; }
}

/// <summary>
/// Inventory metrics by product category
/// </summary>
public class InventoryByCategoryDto
{
    /// <summary>
    /// Product category name
    /// </summary>
    [Required]
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Total products in this category
    /// </summary>
    public int TotalProducts { get; set; }
    
    /// <summary>
    /// Products in stock in this category
    /// </summary>
    public int InStock { get; set; }
    
    /// <summary>
    /// Products with low stock in this category
    /// </summary>
    public int LowStock { get; set; }
    
    /// <summary>
    /// Products out of stock in this category
    /// </summary>
    public int OutOfStock { get; set; }
    
    /// <summary>
    /// Total inventory value for this category
    /// </summary>
    public decimal CategoryValue { get; set; }
}

/// <summary>
/// Individual product inventory information
/// </summary>
public class ProductInventoryDto
{
    /// <summary>
    /// Unique product identifier
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Product name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category
    /// </summary>
    [Required]
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Current stock quantity
    /// </summary>
    public int StockQuantity { get; set; }
    
    /// <summary>
    /// Reorder level threshold
    /// </summary>
    public int ReorderLevel { get; set; }
    
    /// <summary>
    /// Current stock status
    /// </summary>
    [Required]
    public string StockStatus { get; set; } = string.Empty; // "In Stock", "Low Stock", "Out of Stock"
    
    /// <summary>
    /// Unit price of the product
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Total value of current stock (quantity × price)
    /// </summary>
    public decimal StockValue { get; set; }
}
