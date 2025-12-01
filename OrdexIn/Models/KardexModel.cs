using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OrdexIn.Models;

[Table("kardex_test")]
public class KardexModel : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }
    
    [Column("reason")]
    public string Reason { get; set; }
    
    [Column("extended_description")]
    public string? ExtendedDescription { get; set; }
    
    [Column("transaction_type")]
    public string TransactionType { get; set; }
    
    [Column("quantity")]
    public int? Quantity { get; set; }
    
    [Column("unit_cost")]
    public decimal? UnitCost { get; set; }   
    
    [Column("total_cost", ignoreOnInsert: true)]
    public decimal? TotalCost { get; set; }
    
    
    [Column("user_id")]
    public Guid UserId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}