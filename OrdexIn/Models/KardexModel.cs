using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OrdexIn.Models;

public class KardexModel : BaseModel
{
    [PrimaryKey]
    public Guid Id { get; set; }
    
    [Column("product_id")]
    public int ProductId { get; set; }
    
    [Column("transaction_type")]
    public string TransactionType { get; set; }
    
    [Column("quantity")]
    public int Quantity { get; set; }
    
    [Column("unit_cost")]
    public decimal UnitCost { get; set; }   
    
    [Column("total_cost")]
    public decimal TotalCost { get; set; }
    
    [Column("reason")]
    public string Reason { get; set; }
    
    [Column("user_id")]
    public Guid UserId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}