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

        // Obtener por ID
        public async Task<InventarioMinimo?> ObtenerPorId(int id)
        {
            var result = await _client
                .From<InventarioMinimo>()
                .Select("*")
                .Where(i => i.IdInventario == id)
                .Get();

            return result.Models?.FirstOrDefault();
        }

        // Obtener por producto
        public async Task<List<InventarioMinimo>> ObtenerPorProducto(int idProducto)
        {
            var result = await _client
                .From<InventarioMinimo>()
                .Select("*")
                .Where(i => i.IdProducto == idProducto)
                .Get();

            return result.Models ?? new List<InventarioMinimo>();
        }

        // Insertar
        public async Task<InventarioMinimo?> Insertar(InventarioMinimo item)
        {
            var result = await _client
                .From<InventarioMinimo>()
                .Insert(item);

            return result.Models?.FirstOrDefault();
        }

        // Actualizar
        public async Task Actualizar(InventarioMinimo item)
        {
            await _client
                .From<InventarioMinimo>()
                .Where(i => i.IdInventario == item.IdInventario)
                .Update(item);
        }

        // Eliminar
        public async Task Eliminar(int id)
        {
            await _client
                .From<InventarioMinimo>()
                .Where(i => i.IdInventario == id)
                .Delete();
        }

        // Regla de negocio: Calcular mínimo sugerido según precio, demanda o cantidad actual
        public int CalcularInventarioMinimo(int ventasPromMensuales)
        {
            // Regla sencilla
            int min = (int)Math.Ceiling(ventasPromMensuales * 0.20);
            return Math.Max(min, 1);
        }
    }
}
