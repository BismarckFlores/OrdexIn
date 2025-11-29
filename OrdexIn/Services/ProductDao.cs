using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using OrdexIn.Models;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Services
{
    public class ProductDao : IProductService
    {
        private readonly Client _client;
        private const int MaxRetry = 3;

        public ProductDao(Client client)
        {
            _client = client;
        }

        // Obtener todos
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var results = new List<Product>();
            const int chunkSize = 1000;
            int from = 0;

            while (true)
            {
                int intentos = 0;

                while (intentos < MaxRetry)
                {
                    try
                    {
                        var response = await _client
                            .From<Product>()
                            .Select("*")
                            .Range(from, from + chunkSize - 1)
                            .Get();

                        var page = response.Models;
                        if (page.Count == 0)
                        {
                            results.Sort((a, b) => a.Id.CompareTo(b.Id));
                            return results;
                        }

                        results.AddRange(page);

                        if (page.Count < chunkSize)
                        {
                            results.Sort((a, b) => a.Id.CompareTo(b.Id));
                            return results; // last page
                        }

                        from += chunkSize;
                        break; // next page
                    }
                    catch (Exception ex)
                    {
                        intentos++;
                        if (intentos == MaxRetry)
                            throw new Exception($"[ERROR] GetAllProductsAsync failed: {ex.Message}");
                    }
                }
            }
        }

        // Obtener por ID
        public async Task<Product?> GetByIdAsync(int id)
        {
            var intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    var result = await _client
                        .From<Product>()
                        .Select("*")
                        .Where(p => p.Id == id)
                        .Get();

                    return result.Models.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    intentos++;
                    if (intentos == MaxRetry)
                        throw new Exception($"[ERROR] GetByIdAsync failed: {ex.Message}");
                }
            }

            return null;
        }

        // Crear producto
        public async Task<bool> CreateAsync(Product item)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    await _client
                        .From<Product>()
                        .Insert(item);

                    return true;
                }
                catch (Exception ex)
                {
                    intentos++;
                    if (intentos == MaxRetry)
                        throw new Exception($"[ERROR] CreateAsync Falló: {ex.Message}");
                }
            }

            return false;
        }
        
        // Actualizar producto
        public async Task<bool> UpdateAsync(Product item)
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
                    if (intentos == MaxRetry)
                        throw new Exception($"[ERROR] Update failed: {ex.Message}");
                }
            }

            return false;
        }
        
        // Eliminar producto
        public async Task<bool> RemoveAsync(int id)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    await _client
                        .From<Product>()
                        .Where(p => p.Id == id)
                        .Delete();

                    return true;
                }
                catch (Exception ex)
                {
                    intentos++;
                    if (intentos == MaxRetry)
                        throw new Exception($"[ERROR] RemoveAsync failed: {ex.Message}");
                }
            }

            return false;
        }

        // Verificar existencia
        public async Task<bool> ExistsAsync(int id)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    var res = await _client
                        .From<Product>()
                        .Select("*")
                        .Where(p => p.Id == id)
                        .Get();

                    return res.Models.Count > 0;
                }
                catch (Exception ex)
                {
                    intentos++;
                    if (intentos == MaxRetry)
                        throw new Exception($"[ERROR] ExistsAsync failed: {ex.Message}");
                }
            }
            
            return false;
        }
        
        // Contar productos
        public async Task<int> GetTotalInventoryAsync()
        {
            var prodList = await GetAllProductsAsync();
            
            return prodList?.Sum(p => p.Stock) ?? 0;
        }
    }
}
