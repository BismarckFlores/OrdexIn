namespace OrdexIn.Models.DTO;

public class KardexDisplayDto
{
    public Guid KardexId { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Total { get; set; }
    public required string UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
}