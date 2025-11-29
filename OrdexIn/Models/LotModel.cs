using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OrdexIn.Models;

[Table("inventory_lots_test")]
public class LotModel : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }
    
    [Column("product_id")]
    public int ProductId { get; set; }
    
    [Column("quantity")]
    public int Quantity { get; set; }
    
    [Column("expires_at")]
    public DateTime ExpirationDate { get; set; }
}