using OrdexIn.Models;
using Supabase;
using Supabase.Interfaces;

namespace OrdexIn.Services
{
    namespace OrdexIn.Services
    {
        public class PuntoVentaDao
        {
            private readonly Client _SupabaseClient;
            private const int MaxRetry = 3;

            public PuntoVentaDao(Client client)
            {
                _SupabaseClient = client;
            }

            // ==========================================================
            // 1. Obtener Inventario en orden FIFO
            // ==========================================================
            public async Task<List<ProductModel>> GetInventoryFIFO(int idProducto)
            {
                int intentos = 0;

                while (intentos < MaxRetry)
                {
                    try
                    {
                        var result = await _SupabaseClient
                            .From<ProductModel>()
                            .Where(p => p.Id == idProducto)
                            .Order(p => p.ExpirationDate,
                                   Supabase.Postgrest.Constants.Ordering.Ascending)
                            .Get();

                        return result.Models ?? new List<ProductModel>();
                    }
                    catch (Exception ex)
                    {
                        intentos++;
                        Console.WriteLine($"[ERROR] GetInventoryFIFO(): {ex.Message}");

                        if (intentos == MaxRetry)
                            return new List<ProductModel>();
                    }
                }

                return new List<ProductModel>();
            }

            // ==========================================================
            // 2. Registrar Venta (FIFO + Caducidad)
            // ==========================================================
            public async Task<string> RegisterSale(int idProducto, int cantidad)
            {
                try
                {
                    var inventario = await GetInventoryFIFO(idProducto);

                    if (!inventario.Any())
                        return "Producto no encontrado.";

                    int restante = cantidad;

                    foreach (var lote in inventario)
                    {
                        // Saltar lotes vencidos
                        if (lote.ExpirationDate.HasValue &&
                            lote.ExpirationDate.Value < DateTime.UtcNow)
                            continue;

                        // Si el lote cubre todo
                        if (lote.Stock >= restante)
                        {
                            lote.Stock -= restante;

                            if (!await SafeUpdate(lote))
                                return "Error al actualizar el inventario.";

                            return "Venta registrada con éxito.";
                        }
                        else
                        {
                            // Usar todo el lote
                            restante -= lote.Stock;
                            lote.Stock = 0;

                            if (!await SafeUpdate(lote))
                                return "Error al actualizar el inventario.";
                        }
                    }

                    return "Stock insuficiente.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] RegisterSale(): {ex.Message}");
                    return "Error interno al procesar la venta.";
                }
            }

            // ==========================================================
            // 3. Safe Update (modo seguro con reintentos)
            // ==========================================================
            private async Task<bool> SafeUpdate(ProductModel producto)
            {
                int intentos = 0;

                while (intentos < MaxRetry)
                {
                    try
                    {
                        await _SupabaseClient
                            .From<ProductModel>()
                            .Where(p => p.Id == producto.Id)
                            .Update(producto);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        intentos++;
                        Console.WriteLine($"[ERROR] SafeUpdate(): {ex.Message}");

                        if (intentos == MaxRetry)
                            return false;
                    }
                }

                return false;
            }
        }
    }
}
