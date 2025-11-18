using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OrdexIn.Models
{
    [Table("inventario")]
    public class InventarioMinimo : BaseModel
    {
        [PrimaryKey("id_inventario", false)]
        public int IdInventario { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad_minima")]
        public int CantidadMinima { get; set; }
    }
}
