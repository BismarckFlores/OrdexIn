using OrdexIn.Models;

namespace OrdexIn.Services.Intefaces;

public interface IPointOfSaleService
{
    Task<List<LotModel>> GetInventoryAsync(int productId);
    Task<bool> RegisterSaleAsync(ProductModel productModel, int quantity, Guid userId);
    Task<bool> UpdateInventoryAsync(ProductModel productModel, int quantitySold, Guid userId);
    Task<bool> AddNewBatchAsync(ProductModel productModel, LotModel batch, Guid userId, bool createdWhithProduct = false);
    
    Task<bool> TryReserveStockAsync(int productId, int quantity);
    Task<bool> RemoveBatchAsync(LotModel batch, Guid userId);
    Task<bool> RemoveAllBatchFromProductIdAsync(int productId);
    Task<bool> UpdateBatchSafely(LotModel batch, Guid userId);
    
    int GetAvailableStockAsync(List<LotModel> inventory);
}