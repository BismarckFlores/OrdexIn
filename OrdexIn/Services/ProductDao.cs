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
        private readonly Client _client;
        private const int MaxRetry = 3;

        public ProductDao(Client client)
        {
            _client = client;
        }

        // Obtener todos
        public async Task<List<Product>> ObtenerTodos()
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    var result = await _client
                        .From<Product>()
                        .Select("*")
                        .Get();

                    return result.Models ?? new List<Product>();
                }
                catch (Exception ex)
                {
                    intentos++;
                    Console.WriteLine($"[ERROR] ObtenerTodos Falló: {ex.Message}");

                    if (intentos == MaxRetry)
                        return new List<Product>();
                }
            }

            return new List<Product>();
        }

        // Obtener por ID
        public async Task<Product?> ObtenerPorId(int id)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    var result = await _client
                        .From<Product>()
                        .Select("*")
                        .Where(p => p.Id == id)
                        .Get();

                    return result.Models?.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    intentos++;
                    Console.WriteLine($"[ERROR] ObtenerPorId Falló: {ex.Message}");
                    if (intentos == MaxRetry)
                        return null;
                }
            }

            return null;
        }

        // Actualizar producto
        public async Task<bool> Actualizar(Product item)
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
                    Console.WriteLine($"[ERROR] Actualizar Falló: {ex.Message}");
                    if (intentos == MaxRetry)
                        return false;
                }
            }

            return false;
        }
    }
}
