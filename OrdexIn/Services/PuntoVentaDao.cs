using OrdexIn.Models;
using Supabase;
using Supabase.Interfaces;

namespace OrdexIn.Services
{
    public class PuntoVentaDao
    {
        private readonly Client _client;
        private const int MaxRetry = 3;

        public PuntoVentaDao(Client client)
        {
            _client = client;
        }

        // Obtener inventario ordenado por fecha
        public async Task<List<Product>> ObtenerInventario(int idProducto)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    var result = await _client
                        .From<Product>()
                        .Select("*")
                        .Where(p => p.Id == idProducto)
                        .Order(p => p.ExpirationDate, Supabase.Postgrest.Constants.Ordering.Ascending)
                        .Get();

                    return result.Models ?? new List<Product>();
                }
                catch (Exception ex)
                {
                    intentos++;
                    Console.WriteLine($"[ERROR] ObtenerInventario: {ex.Message}");

                    if (intentos == MaxRetry)
                        return new List<Product>();
                }
            }

            return new List<Product>();
        }

        // Registrar venta con FIFO y caducidad
        public async Task<string> RegistrarVenta(int idProducto, int cantidad)
        {
            try
            {
                var inventario = await ObtenerInventario(idProducto);

                if (!inventario.Any())
                    return "Producto no encontrado.";

                int restante = cantidad;

                foreach (var lote in inventario)
                {
                    if (lote.ExpirationDate.HasValue &&
                        lote.ExpirationDate.Value < DateTime.UtcNow)
                        continue; // ignora caducados

                    if (lote.Stock >= restante)
                    {
                        lote.Stock -= restante;

                        if (!await ActualizarProductoSeguro(lote))
                            return "Error actualizando el inventario.";

                        return "Venta realizada con éxito.";
                    }
                    else
                    {
                        restante -= lote.Stock;
                        lote.Stock = 0;

                        if (!await ActualizarProductoSeguro(lote))
                            return "Error actualizando el inventario.";
                    }
                }

                return "No hay suficiente inventario disponible.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RegistrarVenta: {ex.Message}");
                return "Error interno al procesar la venta.";
            }
        }

        // Método auxiliar seguro para actualizar productos
        private async Task<bool> ActualizarProductoSeguro(Product item)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    await _client
                        .From<Product>()
                        .Where(p => p.Id == item.Id)
                        .Update(item);

                    return true;
                }
                catch (Exception ex)
                {
                    intentos++;
                    Console.WriteLine($"[ERROR] ActualizarProductoSeguro: {ex.Message}");

                    if (intentos == MaxRetry)
                        return false;
                }
            }

            return false;
        }
    }
}
