namespace OrdexIn.Models.DTO;

public class InventoryStatsDto
{
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int LowStockCount { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}