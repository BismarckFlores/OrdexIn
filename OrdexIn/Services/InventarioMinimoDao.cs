namespace OrdexIn.Services
{
    using OrdexIn.Models;
    public class InventarioMinimoDao
    {
        public Inventario Crear(int idProducto, int cantidadMinima)
        {
            return new Inventario
            {
                IdProducto = idProducto,
                CantidadMinima = cantidadMinima
            };
        }
    }
}
