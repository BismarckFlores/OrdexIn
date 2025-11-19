using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OrdexIn.Models
{
    [Table("invtest")]
    public class InventarioMinimo : BaseModel
    {
        [PrimaryKey("id_inventario", false)]
        public int IdInventario { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("product_name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column ("amount")]
        public int Amount { get; set; }

        [Column("cantidad_minima")]
        public int CantidadMinima { get; set; }
    }
}
