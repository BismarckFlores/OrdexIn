using OrdexIn.Models;

namespace OrdexIn.Services.Intefaces
{
    public interface IKardexDataService
    {
        Task<List<KardexModel>> GetAllKardexEntriesAsync();
        Task RecordKardexEntryAsync(KardexModel kardexEntry);
    }
}