namespace FabrikamContracts.DTOs.Products;

/// <summary>
/// Inventory status information
/// </summary>
public class InventoryStatusDto
{
    public List<ProductDto> LowStockProducts { get; set; } = new();
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
}
