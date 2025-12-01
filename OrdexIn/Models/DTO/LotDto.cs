namespace OrdexIn.Models;

public class LotDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpirationDate { get; set; }
}