using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OrdexIn.Models
{
    [Table("inventario_minimo")]
    public class InventarioMinimo : BaseModel
    {
        [PrimaryKey("id_inventario_minimo", false)]
        public int IdInventarioMinimo { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad_minima")]
        public int CantidadMinima { get; set; }
    }
}
