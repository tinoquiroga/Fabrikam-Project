using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.DTOs;

/// <summary>
/// Comprehensive product catalog with detailed information
/// </summary>
public class ProductCatalogDto
{
    /// <summary>
    /// High-level catalog summary
    /// </summary>
    public ProductSummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Products organized by category
    /// </summary>
    public List<ProductCategoryDto> Categories { get; set; } = new();
    
    /// <summary>
    /// Featured or highlighted products
    /// </summary>
    public List<ProductDetailDto> FeaturedProducts { get; set; } = new();
}

/// <summary>
/// Overall product catalog summary
/// </summary>
public class ProductSummaryDto
{
    /// <summary>
    /// Total number of products in catalog
    /// </summary>
    public int TotalProducts { get; set; }
    
    /// <summary>
    /// Number of active/available products
    /// </summary>
    public int ActiveProducts { get; set; }
    
    /// <summary>
    /// Number of product categories
    /// </summary>
    public int TotalCategories { get; set; }
    
    /// <summary>
    /// Price range across all products
    /// </summary>
    public PriceRangeDto PriceRange { get; set; } = new();
}

/// <summary>
/// Product category with contained products
/// </summary>
public class ProductCategoryDto
{
    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Category description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of products in this category
    /// </summary>
    public int ProductCount { get; set; }
    
    /// <summary>
    /// Products in this category
    /// </summary>
    public List<ProductDetailDto> Products { get; set; } = new();
}

/// <summary>
/// Detailed product information
/// </summary>
public class ProductDetailDto
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
    /// Product description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category
    /// </summary>
    [Required]
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Current stock quantity
    /// </summary>
    public int StockQuantity { get; set; }
    
    /// <summary>
    /// Stock availability status
    /// </summary>
    public bool IsAvailable { get; set; }
    
    /// <summary>
    /// Product specifications and features
    /// </summary>
    public ProductSpecificationsDto Specifications { get; set; } = new();
    
    /// <summary>
    /// Related asset information (images, brochures, etc.)
    /// </summary>
    public List<ProductAssetDto> Assets { get; set; } = new();
}

/// <summary>
/// Product specifications and technical details
/// </summary>
public class ProductSpecificationsDto
{
    /// <summary>
    /// Material composition
    /// </summary>
    public string Material { get; set; } = string.Empty;
    
    /// <summary>
    /// Product dimensions
    /// </summary>
    public string Dimensions { get; set; } = string.Empty;
    
    /// <summary>
    /// Product weight
    /// </summary>
    public string Weight { get; set; } = string.Empty;
    
    /// <summary>
    /// Color options available
    /// </summary>
    public List<string> Colors { get; set; } = new();
    
    /// <summary>
    /// Additional features and benefits
    /// </summary>
    public List<string> Features { get; set; } = new();
    
    /// <summary>
    /// Warranty information
    /// </summary>
    public string Warranty { get; set; } = string.Empty;
}

/// <summary>
/// Product-related asset information
/// </summary>
public class ProductAssetDto
{
    /// <summary>
    /// Asset type (image, brochure, manual, etc.)
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Asset file name
    /// </summary>
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Asset description or alt text
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Relative path to asset file
    /// </summary>
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Price range information
/// </summary>
public class PriceRangeDto
{
    /// <summary>
    /// Lowest price in catalog
    /// </summary>
    public decimal MinPrice { get; set; }
    
    /// <summary>
    /// Highest price in catalog
    /// </summary>
    public decimal MaxPrice { get; set; }
    
    /// <summary>
    /// Average price across all products
    /// </summary>
    public decimal AveragePrice { get; set; }
}
