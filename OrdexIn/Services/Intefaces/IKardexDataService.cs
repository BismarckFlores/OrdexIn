using OrdexIn.Models;

namespace OrdexIn.Services.Intefaces
{
    public interface IKardexDataService
    {
        Task<List<KardexModel>> GetAllKardexEntriesAsync();
        Task RecordKardexEntryAsync(KardexModel kardexEntry);

        Task RegisterKardexEntry(string transectionType, Guid userId, string? reason = null, int? productId = null,
            string? productName = null, int? batchId = null, int? quantity = null, decimal? unitCost = null,
            DateTime? expiration = null);

        Task RegisterKardexEntry(int quantitySold, decimal price, string transectionType,
            Guid userId, string? reason = null, int? productId = null, string? productName = null,
            int? batchId = null, DateTime? expiration = null);
    }
}