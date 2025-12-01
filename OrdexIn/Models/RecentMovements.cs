namespace OrdexIn.Models;

public class RecentMovements
{
    public string TransactionType { get; set; } = string.Empty;
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? Total { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}