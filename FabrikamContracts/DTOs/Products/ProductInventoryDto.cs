namespace FabrikamContracts.DTOs.Products;

/// <summary>
/// Product inventory information
/// </summary>
public class ProductInventoryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public bool NeedsReorder { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
}
