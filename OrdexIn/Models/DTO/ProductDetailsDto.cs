namespace OrdexIn.Models.DTO;

public class ProductDetailsDto
{
    public ProductModel ProductModel { get; set; } = new ();
    public List<LotModel> Batches { get; set; } = new ();   
}