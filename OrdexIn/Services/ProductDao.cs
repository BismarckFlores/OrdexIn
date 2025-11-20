using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using OrdexIn.Models;

namespace OrdexIn.Services
{
    public class ProductDao
    {
        private readonly Client _SupabaseClient;

        public ProductDao(Client client)
        {
            _SupabaseClient = client;
        }

        // Obtener todos
        public async Task<List<Product>> ObtenerTodos()
        {
            var response = await _SupabaseClient.From<Product>().Select("*").Get();
            return response.Models ?? new List<Product>();
        }

        // Obtener por ID
        public async Task<Product?> ObtenerPorId(int id)
        {
            var response = await _SupabaseClient
                .From<Product>()
                .Select("*")
                .Where(p => p.Id == id)
                .Get();

            return response.Models?.FirstOrDefault();
        }

       
        public async Task<Product?> Insertar(Product item)
        {
            var response = await _SupabaseClient.From<Product>().Insert(item);
            return response.Models?.FirstOrDefault();
        }

        
        public async Task Actualizar(Product item)
        {
            await _SupabaseClient
                .From<Product>()
                .Where(p => p.Id == item.Id)
                .Update(item);
        }

        
        public async Task Eliminar(int id)
        {
            await _SupabaseClient
                .From<Product>()
                .Where(p => p.Id == id)
                .Delete();
        }

        
        public async Task<List<Product>> ObtenerBajoMinimo()
        {
            var all = await ObtenerTodos();
            return all.Where(p => p.Stock <= p.StockMin).ToList();
        }
    }
}
