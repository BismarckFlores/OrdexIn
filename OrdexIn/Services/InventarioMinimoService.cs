namespace OrdexIn.Services
{
    using OrdexIn.Models;
    public class InventarioMinimoService
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
