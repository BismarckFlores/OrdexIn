using OrdexIn.Models;
using OrdexIn.Services.Intefaces;
using Supabase;

namespace OrdexIn.Services;

public class KardexDAO : IKardexDataService
{
    private readonly Client _supabaseClient;
    public KardexDAO(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<List<KardexModel>> GetAllKardexEntriesAsync()
    {
        try
        {
            var kardexEntries = new List<KardexModel>();
            const int chunkSize = 1000;
            int offset = 0;
            while (true)
            {
                var response = await _supabaseClient
                    .From<KardexModel>()
                    .Range(offset, offset + chunkSize - 1)
                    .Get();

                var pageEntries = response.Models ?? new List<KardexModel>();
                if (pageEntries.Count == 0)
                    break;
                kardexEntries.AddRange(pageEntries);
                if (pageEntries.Count < chunkSize)
                    break;
                offset += chunkSize;
            }
            
            kardexEntries.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            return kardexEntries;
        }

        catch (Exception ex) 
        {
            throw new Exception("Error fetching kardex entries", ex);

        }
       
    }

    
    public async Task RecordKardexEntryAsync(KardexModel kardexEntry)
    {
        try 
        { 
            await _supabaseClient
                .From<KardexModel>()
                .Insert(kardexEntry);
        }
        catch (Exception ex)
        {
            // Handle exceptions appropriately
            throw new Exception("Error registering entry", ex);
        }
    }
}