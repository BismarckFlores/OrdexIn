using Supabase;
using System.Linq;

namespace OrdexIn.Services
{
    using OrdexIn.Models;
    public class InventarioMinimoDao
    {
        private readonly Client _client;

        public InventarioMinimoDao(Client client)
        {
            _client = client;
        }

        // Obtener todos los registros
        public async Task<List<InventarioMinimo>> ObtenerTodos()
        {
            var result = await _client
                .From<InventarioMinimo>()
                .Select("*")
                .Get();

            return result.Models ?? new List<InventarioMinimo>();
        }

        // Obtener por producto
        public async Task<InventarioMinimo?> ObtenerPorProducto(int idProducto)
        {
            var result = await _client
                .From<InventarioMinimo>()
                .Select("*")
                .Where(i => i.IdProducto == idProducto)
                .Get();

            return result.Models?.FirstOrDefault();
        }

        // Obtener por ID
        public async Task<InventarioMinimo?> ObtenerPorId(int id)
        {
            var result = await _client
                .From<InventarioMinimo>()
                .Select("*")
                .Where(i => i.IdInventarioMinimo == id)
                .Get();

            return result.Models?.FirstOrDefault();
        }

        // Insertar
        public async Task<InventarioMinimo?> Insertar(InventarioMinimo item)
        {
            var result = await _client
                .From<InventarioMinimo>()
                .Insert(item);

            return result.Models?.FirstOrDefault();
        }

        // Actualizar (SUPABASE UPDATE = void en algunas versiones)
        public async Task Actualizar(InventarioMinimo item)
        {
            await _client
                .From<InventarioMinimo>()
                .Where(i => i.IdInventarioMinimo == item.IdInventarioMinimo)
                .Update(item);
        }

        // Eliminar (SUPABASE DELETE = void en algunas versiones)
        public async Task Eliminar(int id)
        {
            await _client
                .From<InventarioMinimo>()
                .Where(i => i.IdInventarioMinimo == id)
                .Delete();
        }

        // Regla de negocio: calcular inventario mínimo recomendado
        public int CalcularInventarioMinimo(int ventasPromedioMensuales)
        {
            int minimo = (int)Math.Ceiling(ventasPromedioMensuales * 0.20);
            return Math.Max(minimo, 1);
        }
    }
}
