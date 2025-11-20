using OrdexIn.Models;
using Supabase;
using Supabase.Interfaces;

namespace OrdexIn.Services
{
    public class PuntoVentaDao
    {
        private readonly Client _SupabaseClient;

        public PuntoVentaDao(Client client)
        {
            _SupabaseClient = client;
        }

       
        public async Task<(bool exito, string mensaje)> ProcesarVenta(int productId, int cantidadVendida)
        {
            
            var response = await _SupabaseClient
                .From<Product>()
                .Select("*")
                .Where(p => p.Id == productId)
                .Get();

            var product = response.Models?.FirstOrDefault();

            if (product == null)
                return (false, "Producto no encontrado.");

            
            if (cantidadVendida <= 0)
                return (false, "La cantidad vendida debe ser mayor a 0.");

            if (product.Stock < cantidadVendida)
                return (false, "Stock insuficiente para completar la venta.");

           
            if (product.ExpirationDate < DateTime.UtcNow)
                return (false, "El producto está caducado y NO puede venderse.");

           
            product.Stock -= cantidadVendida;

            await _SupabaseClient
                .From<Product>()
                .Where(p => p.Id == product.Id)
                .Update(product);

            return (true, "Venta procesada correctamente.");
        }

        
        public async Task<List<Product>> ObtenerProductosCaducados()
        {
            var response = await _SupabaseClient.From<Product>().Select("*").Get();

            return response.Models?
                .Where(p => p.ExpirationDate < DateTime.UtcNow)
                .ToList() ?? new List<Product>();
        }

        
        public async Task<List<Product>> ObtenerProximosACaducar(int dias)
        {
            var now = DateTime.UtcNow;
            var limite = now.AddDays(dias);

            var response = await _SupabaseClient.From<Product>().Select("*").Get();

            return response.Models?
                .Where(p => p.ExpirationDate >= now && p.ExpirationDate <= limite)
                .ToList() ?? new List<Product>();
        }

        
        public async Task<(decimal total, string mensaje)> CalcularTotal(int productId, int cantidad)
        {
            var response = await _SupabaseClient
                .From<Product>()
                .Select("*")
                .Where(p => p.Id == productId)
                .Get();

            var product = response.Models?.FirstOrDefault();

            if (product == null)
                return (0, "Producto no encontrado.");

            if (cantidad <= 0)
                return (0, "Cantidad inválida.");

            if (cantidad > product.Stock)
                return (0, "Cantidad excede el stock disponible.");

            return (product.Price * cantidad, "OK");
        }
    }
}
