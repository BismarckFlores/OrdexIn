using OrdexIn.Models;
using Supabase.Postgrest;
using Client = Supabase.Client;

namespace OrdexIn.Services.Intefaces;

public class PointSaleDAO : IPointOfSaleService
{
    private readonly Client _client;
    private readonly IKardexDataService _kardexService;
    private readonly ILogger<PointSaleDAO> _logger;
    private const int MaxRetry = 3;

    public PointSaleDAO(Client client, IKardexDataService kardexService, ILogger<PointSaleDAO> logger)
    {
        _client = client;
        _kardexService = kardexService;
        _logger = logger;
    }

    public async Task<List<LotModel>> GetInventoryAsync(int productId)
    {
        var attempts = 0;
        while (attempts < MaxRetry)
        {
            try
            {
                var res = await _client
                    .From<LotModel>()
                    .Select("*")
                    .Where(l => l.ProductId == productId)
                    .Get();

                return res.Models;
            }
            catch (Exception ex)
            {
                attempts++;
                _logger?.LogWarning(ex, "Attempt {Attempt} failed getting lots for product {ProductId}", attempts,
                    productId);
                if (attempts == MaxRetry)
                {
                    _logger?.LogError(ex, "Failed to get lots for product {ProductId} after {MaxRetry} attempts",
                        productId, MaxRetry);
                    throw new Exception($"Failed to get lots for product {productId}", ex);
                }

                await Task.Delay(100 * attempts);
            }
        }

        return new List<LotModel>();
    }

    public async Task<bool> RegisterSaleAsync(ProductModel productModel, int quantity, Guid userId)
    {
        try
        {
            if (!await UpdateInventoryAsync(productModel, quantity, userId)) return false;
            await _kardexService.RegisterKardexEntry(
                quantity,
                productModel.Price,
                "OUT",
                userId,
                productId: productModel.Id,
                productName: productModel.Name
            );
            return true;
        }
        catch
        {
            throw new Exception($"Failed to register sale for product {productModel.Id}");
        }
    }

    public async Task<bool> UpdateInventoryAsync(ProductModel productModel, int quantitySold, Guid userId)
    {
        var inv = await GetInventoryAsync(productModel.Id);

        var availableStock = GetAvailableStockAsync(inv);
        if (availableStock < quantitySold) return false;

        var quantityToSell = quantitySold;
        var updatedBatches = new List<LotModel>();

        // Consume FIFO - ensure ordering by CreatedAt (older first)
        var ordered = inv.OrderBy(b => b.CreatedAt).ToList();

        foreach (var batch in ordered)
        {
            if (quantityToSell <= 0) break;

            // If batch has an expiration date and is expired => remove that single batch
            if (batch.ExpirationDate <= DateTime.UtcNow)
            {
                // remove only this batch
                await RemoveBatchAsync(batch, userId);
                // continue regardless of removed success; expired batch doesn't contribute to sale
                continue;
            }

            if (batch.Quantity <= 0) continue;

            var quantityFromBatch = Math.Min(batch.Quantity, quantityToSell);
            if (quantityFromBatch <= 0) break;

            batch.Quantity -= quantityFromBatch;
            updatedBatches.Add(batch);

            quantityToSell -= quantityFromBatch;
        }

        if (quantityToSell > 0)
            return false;

        // persist updates (or removals) for each updated batch
        foreach (var batch in updatedBatches)
        {
            var success = await UpdateBatchSafely(batch, userId);
            if (!success)
                return false;
        }

        return true;
    }

    public async Task<bool> AddNewBatchAsync(ProductModel productModel, LotModel batch, Guid userId,
        bool createdWhithProduct = false)
    {
        var attempts = 0;
        while (attempts < MaxRetry)
        {
            try
            {
                await _client
                    .From<LotModel>()
                    .Insert(batch);

                if (createdWhithProduct)
                {
                    await _kardexService.RegisterKardexEntry(
                        "ADD",
                        userId,
                        productId: batch.ProductId,
                        productName: productModel.Name,
                        quantity: batch.Quantity,
                        expiration: batch.ExpirationDate
                    );
                }
                else
                {
                    await _kardexService.RegisterKardexEntry(
                        "IN",
                        userId,
                        productId: batch.ProductId,
                        productName: productModel.Name,
                        batchId: batch.Id,
                        quantity: batch.Quantity,
                        expiration: batch.ExpirationDate
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                attempts++;
                if (attempts == MaxRetry)
                    throw new Exception($"Failed to create the batch {batch.Id}", ex);
                await Task.Delay(100 * attempts);
            }

        }

        return false;
    }

    public int GetAvailableStockAsync(List<LotModel> inventory)
    {
        if (inventory.Count == 0) return 0;

        var sum = inventory
            .Where(batch => batch.ExpirationDate == default || batch.ExpirationDate >= DateTime.UtcNow)
            .Sum(batch => batch.Quantity);
        return sum;
    }

    public async Task<bool> TryReserveStockAsync(int productId, int quantity)
    {
        var inv = await GetInventoryAsync(productId);
        return GetAvailableStockAsync(inv) >= quantity;
    }

    public async Task<bool> RemoveBatchAsync(LotModel batch, Guid userId)
    {
        var model = await _client
            .From<ProductModel>()
            .Where(p => p.Id == batch.ProductId)
            .Single();
        if (model == null)
            throw new Exception($"Product {batch.ProductId} not found for batch removal");
        
        var attempts = 0;
        while (attempts < MaxRetry)
        {
            try
            {
                var res = await _client
                    .From<LotModel>()
                    .Where(b => b.Id == batch.Id)
                    .Single();
                
                if (res == null)
                    throw new Exception($"Batch {batch.Id} not found for removal");

                await _kardexService.RegisterKardexEntry(
                    "DEL",
                    userId,
                    productId: batch.ProductId,
                    productName: model.Name,
                    batchId: batch.Id,
                    quantity: batch.Quantity,
                    expiration: batch.ExpirationDate
                );
                
                await res.Delete<LotModel>();

                return true;
            }
            catch (Exception ex)
            {
                attempts++;
                if (attempts == MaxRetry)
                    throw new Exception($"Failed to remove batch {batch.Id} for product {batch.ProductId}", ex);
                await Task.Delay(100 * attempts);
            }
        }

        return false;
    }

    public async Task<bool> RemoveAllBatchFromProductIdAsync(int productId)
    {
        var attempts = 0;
        while (attempts < MaxRetry)
        {
            try
            {
                await _client
                    .From<LotModel>()
                    .Where(b => b.ProductId == productId)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                attempts++;
                if (attempts == MaxRetry)
                    throw new Exception($"Failed to remove batches for product {productId}", ex);
                await Task.Delay(100 * attempts);
            }
        }

        return false;
    }

    public async Task<bool> UpdateBatchSafely(LotModel batch, Guid userId)
    {
        if (batch.Quantity <= 0)
            return await RemoveBatchAsync(batch, userId);

        var attempts = 0;
        while (attempts < MaxRetry)
        {
            try
            {
                var batchUpdate = await _client
                    .From<LotModel>()
                    .Where(b => b.ProductId == batch.ProductId)
                    .Single();

                if (batchUpdate == null)
                    throw new Exception($"Batch {batch.Id} not found for update");

                batchUpdate.Quantity = batch.Quantity;
                await batchUpdate.Update<LotModel>();

                return true;
            }
            catch (Exception ex)
            {
                attempts++;
                if (attempts == MaxRetry)
                    throw new Exception($"Failed to update batch {batch.Id}", ex);
                await Task.Delay(100 * attempts);
            }
        }

        return false;
    }
}