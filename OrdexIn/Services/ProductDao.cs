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
        private const int MaxRetry = 3;

        public ProductDao(Client client)
        {
            _SupabaseClient = client;
        }

        // CREATE
        public async Task<ProductModel?> Create(ProductModel producto)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    var result = await _SupabaseClient
                        .From<ProductModel>()
                        .Insert(producto);

                    return result.Models.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    intentos++;
                    Console.WriteLine($"[ERROR] Crear(): {ex.Message}");
                    if (intentos == MaxRetry)
                        return null;
                }
            }

            return null;
        }

        // READ - Todos
        public async Task<List<ProductModel>> GetAll()
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    var result = await _SupabaseClient
                        .From<ProductModel>()
                        .Select("*")
                        .Get();

                    return result.Models ?? new List<ProductModel>();
                }
                catch (Exception ex)
                {
                    intentos++;
                    Console.WriteLine($"[ERROR] ObtenerTodos(): {ex.Message}");
                    if (intentos == MaxRetry)
                        return new List<ProductModel>();
                }
            }

            return new List<ProductModel>();
        }

        // READ - Por ID
        public async Task<ProductModel?> GetForId(int id)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    var result = await _SupabaseClient
                        .From<ProductModel>()
                        .Where(p => p.Id == id)
                        .Get();

                    return result.Models.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    intentos++;
                    Console.WriteLine($"[ERROR] ObtenerPorId(): {ex.Message}");
                    if (intentos == MaxRetry)
                        return null;
                }
            }

            return null;
        }

        // UPDATE
        public async Task<bool> Update(ProductModel producto)
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
                    Console.WriteLine($"[ERROR] Actualizar(): {ex.Message}");
                    if (intentos == MaxRetry)
                        return false;
                }
            }

            return false;
        }

        // DELETE
        public async Task<bool> Delete(int id)
        {
            int intentos = 0;

            while (intentos < MaxRetry)
            {
                try
                {
                    await _SupabaseClient
                        .From<ProductModel>()
                        .Where(p => p.Id == id)
                        .Delete();

                    return true;
                }
                catch (Exception ex)
                {
                    intentos++;
                    Console.WriteLine($"[ERROR] Eliminar(): {ex.Message}");
                    if (intentos == MaxRetry)
                        return false;
                }
            }

            return false;
        }
    }
}
