namespace FabrikamContracts.DTOs.Products;

/// <summary>
/// Detailed product information
/// </summary>
public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string StockStatus { get; set; } = string.Empty;
    public string? Dimensions { get; set; }
    public int? SquareFeet { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int DeliveryDaysEstimate { get; set; }
    public string Description { get; set; } = string.Empty;
}
