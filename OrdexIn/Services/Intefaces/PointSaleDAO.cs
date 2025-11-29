using OrdexIn.Models;
using Supabase.Postgrest;
using Client = Supabase.Client;

namespace OrdexIn.Services.Intefaces;

public class PointSaleDAO : IPointOfSaleService
{
    private readonly Client _client;
    private readonly IKardexDataService _kardexService;
    private const int MaxRetry = 3;
    
    public PointSaleDAO(Client client, IKardexDataService kardexService)
    {
        _client = client;
        _kardexService = kardexService;
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
                    .Order(p => p.ExpirationDate, Constants.Ordering.Ascending)
                    .Get();
                
                return res.Models;
            }
            catch (Exception ex)
            {
                attempts++;
                if (attempts == MaxRetry)
                    throw new Exception($"Failed to get lots for product {productId}", ex);
                await Task.Delay(100 * attempts);
            }
        }
        return new List<LotModel>();
    }

    public async Task<bool> RegisterSaleAsync(Product product, int quantity, string reason, string transectionType, Guid userId)
    {
        try
        {
            return await UpdateInventoryAsync(product, quantity, reason, transectionType, userId);
        }
        catch
        {
            throw new Exception($"Failed to register sale for product {product.Id}");
        }
    }

    public async Task<bool> UpdateInventoryAsync(Product product, int quantitySold, string reason, string transectionType, Guid userId)
    {
        
        var inv = await GetInventoryAsync(product.Id);
        
        var availableStock = GetAvailableStockAsync(inv);
        if (availableStock < quantitySold) return false;
        
        var quantityToSell = quantitySold;
        var updatedBatches = new List<LotModel>();

        // Consume FIFO
        foreach (var batch in inv)
        {
            if (quantityToSell <= 0) break;
            if (batch.ExpirationDate > DateTime.UtcNow) continue;
            if (batch.Quantity <= 0) continue;
            
            var quantityFromBatch = Math.Min(batch.Quantity, quantityToSell);
            if (quantityFromBatch <= 0) break;
            
            batch.Quantity -= quantityFromBatch;
            updatedBatches.Add(batch);
            
            quantityToSell -= quantityFromBatch;
        }

        if (quantityToSell > 0)
            return false;
        
        foreach (var batch in updatedBatches)
        {
            var success = await UpdateBatchSafely(batch);
            if (!success)
                return false;
        }

        var entry = new KardexModel
        {
            ProductId = product.Id,
            TransactionType = transectionType,
            Quantity = quantitySold,
            UnitCost = product.Price,
            Reason = reason,
            Id = userId
        };

        try
        {
            await _kardexService.RecordKardexEntryAsync(entry);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to record kardex entry", ex);
        }
    }

    public int GetAvailableStockAsync(List<LotModel> inventory)
    {
        if (inventory.Count == 0) return 0;
        
        var sum = inventory.Where(batch => batch.ExpirationDate >= DateTime.UtcNow).Sum(batch => batch.Quantity);
        return sum;
    }
    
    public async Task<bool> TryReserveStockAsync(int productId, int quantity)
    {
        var inv = await GetInventoryAsync(productId);
        var available = GetAvailableStockAsync(inv);
        return available >= quantity;
    }
    
    public async Task<bool> RemoveBatchesAsync(LotModel batch)
    {
        int attempts = 0;
        while (attempts < MaxRetry)
        {
            try
            {
                await _client
                    .From<LotModel>()
                    .Delete(batch);
                
                return true;
            }
            catch (Exception ex)
            {
                attempts++;
                if (attempts == MaxRetry)
                    throw new Exception($"Failed to remove batches for product {batch.ProductId}", ex);
                await Task.Delay(100 * attempts);
            }
        }
        return false;
    }
    
    private async Task<bool> UpdateBatchSafely(LotModel batch)
    {
        if (batch.Quantity <= 0)
            return await RemoveBatchesAsync(batch);
        
        var attempts = 0;
        while (attempts < MaxRetry)
        {
            try
            {
                await _client
                    .From<LotModel>()
                    .Where(b => b.ProductId == batch.ProductId)
                    .Update(batch);
                
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