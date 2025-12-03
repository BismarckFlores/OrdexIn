using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OrdexIn.Hubs;
using Supabase;
using OrdexIn.Models;
using OrdexIn.Models.DTO;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Services
{
    public class ProductDAO : IProductService
    {
        private readonly IHubContext<InventoryHub> _hubContext;
        private readonly IKardexDataService _kardexService;
        private readonly IPointOfSaleService _pointOfSaleService;
        private readonly Client _client;
        private const int MaxRetry = 3;

        public ProductDAO(Client client, IHubContext<InventoryHub> hubContext, IKardexDataService kardexService,
            IPointOfSaleService pointOfSaleService, ILogger<ProductDAO> logger)
        {
            _client = client;
            _hubContext = hubContext;
            _kardexService = kardexService;
            _pointOfSaleService = pointOfSaleService;
        }

        // Obtener todos
        public async Task<List<ProductModel>> GetAllProductsAsync()
        {
            var results = new List<ProductModel>();
            const int chunkSize = 1000;
            int from = 0;

            while (true)
            {
                var response = await ExecuteWithRetry(async () =>
                    await _client
                        .From<ProductModel>()
                        .Select("*")
                        .Range(from, from + chunkSize - 1)
                        .Get()
                );

                var page = response?.Models;
                if (page == null || page.Count == 0)
                    break;

                results.AddRange(page);

                if (page.Count < chunkSize)
                    break; // last page

                from += chunkSize;
            }

            results.Sort((a, b) => a.Id.CompareTo(b.Id));
            return results;
        }

        // Obtener por ID
        public async Task<ProductModel?> GetByIdAsync(int id)
        {
            try
            {
                var result = await ExecuteWithRetry(async () =>
                    {
                        var result = await _client
                            .From<ProductModel>()
                            .Where(p => p.Id == id)
                            .Single();

                        return result;
                    }
                );
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"[ERROR] GetByIdAsync failed: {ex.Message}");
            }
        }

        // Crear producto
        public async Task<ProductModel?> CreateAsync(ProductModel item, Guid userId)
        {
            try
            {
                var result = await ExecuteWithRetry(async () =>
                    {
                        var resp = await _client
                            .From<ProductModel>()
                            .Insert(item);

                        var created = resp?.Models?.FirstOrDefault();
                        if (created == null)
                            throw new Exception("Insert returned no created product");
                        
                        await BroadcastStatsUpdateAsync();
                        await BroadcastProductsUpdateAsync();

                        return created;
                    }
                );
                return result;
            }
            catch (Exception e)
            {
                throw new Exception($"[ERROR] CreateAsync failed: {e.Message}");
            }
        }
        // Actualizar producto
        public async Task<bool> UpdateAsync(ProductDto dto, Guid userId)
        {
            
            try
            {
                var result = await ExecuteWithRetry(async () =>
                    {
                        var productUpdate = await _client
                            .From<ProductModel>()
                            .Where(p => p.Id == dto.Id)
                            .Single();
                    
                        if (productUpdate == null)
                            throw new Exception("Product not found for update");
                        
                        var reason = string.Empty;
                        if (productUpdate.Name != dto.Name) reason += $"Nombre: '{productUpdate.Name}' -> '{dto.Name}'. ";
                        else if (productUpdate.Price != dto.Price) reason += $"Precio: {productUpdate.Price} -> {dto.Price}. ";
                        else if (productUpdate.StockMin != dto.MinStock) reason += $"Stock Minimo: {productUpdate.StockMin} -> {dto.MinStock}. ";
                        else return  true; // No changes detected
                            
                        
                        productUpdate.Name = dto.Name;
                        productUpdate.Price = dto.Price;
                        productUpdate.StockMin = dto.MinStock;
                        await productUpdate.Update<ProductModel>();

                        await _kardexService.RegisterKardexEntry(
                            "MOD",
                            userId,
                            reason,
                            productUpdate.Id,
                            productUpdate.Name
                        );
                        
                        await BroadcastStatsUpdateAsync();
                        await BroadcastProductsUpdateAsync();
                        return true;
                    }
                );
                return result;
            }
            catch (Exception e)
            {
                throw new Exception($"[ERROR] UpdateAsync failed: {e.Message}");
            }
        }
        
        // Eliminar producto
        public async Task<bool> RemoveAsync(int id, Guid userId, bool isRollback = false)
        {
            try
            {
                var name = await ExecuteWithRetry(async () =>
                    {
                        var deletedProduct = await _client
                            .From<ProductModel>()
                            .Where(p => p.Id == id)
                            .Single();
                        
                        if (deletedProduct == null && !isRollback) throw  new Exception("Product not found for delete");
                        if (deletedProduct == null && isRollback) return string.Empty;
                        
                        var prodName = deletedProduct.Name;
                        
                        await _pointOfSaleService.RemoveAllBatchFromProductIdAsync(deletedProduct.Id);
                        await deletedProduct.Delete<ProductModel>();
                        
                        await BroadcastStatsUpdateAsync();
                        await BroadcastProductsUpdateAsync();
                        return prodName;
                    }
                );
                if (!isRollback)
                {
                    await _kardexService.RegisterKardexEntry(
                        "DEL",
                        userId,
                        productId: id,
                        productName: name
                    );
                }
                return true;
            }
            catch (Exception e)
            {
                throw new Exception($"[ERROR] RemoveAsync failed: {e.Message}");
            }
        }

        // Verificar existencia
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var result = await ExecuteWithRetry(async () =>
                    {
                        var res = await _client
                            .From<ProductModel>()
                            .Select("*")
                            .Where(p => p.Id == id)
                            .Get();

                        return res.Models.Count > 0;
                    }
                );
                return result;
            }
            catch (Exception e)
            {
                throw new Exception($"[ERROR] ExistsAsync failed: {e.Message}");
            }
        }
        
        // Contar productos
        public async Task<int> GetTotalInventoryAsync()
        {
            var prodList = await GetAllProductsAsync();
            
            return prodList?.Sum(p => p.Stock) ?? 0;
        }
        
        private async Task BroadcastStatsUpdateAsync()
        {
            var products = await GetAllProductsAsync();
            var stats = new InventoryStatsDto
            {
                TotalProducts = products.Count,
                TotalStock = products.Sum(p => p.Stock),
                TotalInventoryValue = products.Sum(p => p.Stock * p.Price),
                LowStockCount = products.Count(p => p.Stock < 10),
                LastUpdatedUtc = DateTime.UtcNow
            };
            await _hubContext.Clients.All.SendAsync("InventoryUpdated", stats);
        }
        
        private async Task BroadcastProductsUpdateAsync()
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ProductsChanged");
            }
            catch
            {
                // swallow to avoid breaking DB operations if hub fails
            }
        }
        
        private static async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation)
        {
            var attempts = 0;
            var delayMs = 200;

            while (true)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    attempts++;
                    if (attempts >= MaxRetry)
                        throw new Exception($"[ERROR] Operation failed after {MaxRetry} attempts: {ex.Message}", ex);

                    await Task.Delay(delayMs);
                    delayMs = Math.Min(delayMs * 2, 2000);
                }
            }
        }
    }
}