using System.Text.Json.Serialization;

namespace OrdexIn.Models.DTO;

public class ProductCreateDto
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    
    [JsonPropertyName("minStock")]
    public int MinStock { get; set; }
    
    public DateTime ExpirationDate { get; set; }
}