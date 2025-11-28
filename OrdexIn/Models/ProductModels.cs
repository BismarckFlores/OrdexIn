using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace OrdexIn.Models
{
    [Table("inventory_test")]
    public class Product : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        [Column("stock_min")]
        public int StockMin { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expiration_date")]
        public DateTime? ExpirationDate { get; set; }

    }
}
