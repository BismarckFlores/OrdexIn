using OrdexIn.Models;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Services;

public class KardexDAO : IKardexDataService
{
    public Task<List<KardexModel>> GetAllKardexEntriesAsync()
    {
        throw new NotImplementedException();
    }
    
    public Task RecordKardexEntryAsync(KardexModel kardexEntry)
    {
        throw new NotImplementedException();
    }
}