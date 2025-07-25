using System.ComponentModel.DataAnnotations;

namespace FabrikamApi.DTOs;

/// <summary>
/// Product information structure
/// This DTO aligns exactly with ProductsController.GetProducts() API response
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Product ID - matches API property 'id'
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Product name - matches API property 'name'
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Model number - matches API property 'modelNumber'
    /// </summary>
    public string ModelNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category - matches API property 'category'
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Product price - matches API property 'price'
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Stock quantity - matches API property 'stockQuantity'
    /// </summary>
    public int StockQuantity { get; set; }
    
    /// <summary>
    /// Reorder level - matches API property 'reorderLevel'
    /// </summary>
    public int ReorderLevel { get; set; }
    
    /// <summary>
    /// Stock status - matches API property 'stockStatus'
    /// </summary>
    public string StockStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Dimensions - matches API property 'dimensions'
    /// </summary>
    public string? Dimensions { get; set; }
    
    /// <summary>
    /// Square feet - matches API property 'squareFeet'
    /// </summary>
    public int? SquareFeet { get; set; }
    
    /// <summary>
    /// Number of bedrooms - matches API property 'bedrooms'
    /// </summary>
    public int? Bedrooms { get; set; }
    
    /// <summary>
    /// Number of bathrooms - matches API property 'bathrooms'
    /// </summary>
    public int? Bathrooms { get; set; }
    
    /// <summary>
    /// Delivery estimate in days - matches API property 'deliveryDaysEstimate'
    /// </summary>
    public int DeliveryDaysEstimate { get; set; }
}
