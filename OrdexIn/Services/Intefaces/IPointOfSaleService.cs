using OrdexIn.Models;

namespace OrdexIn.Services.Intefaces;

public interface IPointOfSaleService
{
    Task<List<LotModel>> GetInventoryAsync(int productId);
    Task<bool> RegisterSaleAsync(Product product, int quantity, string reason, string transectionType, Guid userId);
    Task<bool> UpdateInventoryAsync(Product product, int quantitySold, string reason, string transectionType, Guid userId);
    
    Task<bool> TryReserveStockAsync(int productId, int quantity);
    Task<bool> RemoveBatchesAsync(LotModel batch);
    
    int GetAvailableStockAsync(List<LotModel> inventory);
}
