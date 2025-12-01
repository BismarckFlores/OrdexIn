namespace OrdexIn.Models;

public class ProductDetailsViewModel
{
    public ProductModel ProductModel { get; set; } = new ();
    public List<LotModel> Batches { get; set; } = new ();   
}