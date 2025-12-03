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

                var pageEntries = response.Models;
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
    
    // Updated RegisterKardexEntry overloads to include product info
    public async Task RegisterKardexEntry(string transectionType, Guid userId, string? reason = null, int? productId = null,
        string? productName = null, int? batchId = null, int? quantity = null, decimal? unitCost = null,
        DateTime? expiration = null)
    {
        // If caller didn't provide a reason, generate a short one automatically (MOD always generate).
        var autoReason = GenerateShortReason(transectionType, productId, productName, batchId, quantity);
        var finalReason = transectionType == "MOD" ? autoReason : string.IsNullOrWhiteSpace(reason) ? autoReason : reason;

        var entry = new KardexModel
        {
            TransactionType = transectionType,
            Reason = finalReason, // placeholder example: "Se añadio el producto {#3, Leche}" when reason was null
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExtendedDescription = BuildExtendedDescription(transectionType, productId, productName,
                quantity, unitCost, finalReason, batchId, expiration)
        };

        await RecordKardexEntryAsync(entry);
    }

    public async Task RegisterKardexEntry(int quantitySold, decimal price, string transectionType,
        Guid userId, string? reason = null, int? productId = null, string? productName = null,
        int? batchId = null, DateTime? expiration = null)
    {
        var autoReason = GenerateShortReason(transectionType, productId, productName, batchId, quantitySold);
        var finalReason = transectionType == "MOD" ? autoReason : string.IsNullOrWhiteSpace(reason) ? autoReason : reason;
        
        var entry = new KardexModel
        {
            TransactionType = transectionType,
            Quantity = quantitySold,
            UnitCost = price,
            Reason = finalReason, // placeholder example: "Se vendieron 50 productos {#3, Leche}" when reason was null
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExtendedDescription = BuildExtendedDescription(transectionType, productId, productName, quantitySold,
                price, finalReason, batchId, expiration)
        };

        await RecordKardexEntryAsync(entry);
    }
    
    
    // Helper generation methods
    private static string GenerateShortReason(string transactionType, int? productId = null,
        string? productName = null, int? batchId = null, int? quantity = null)
    {
        var prodPart = productId.HasValue ? $"{{#{productId.Value}, {productName}}}" : (productName != null ? $"{{{productName}}}" : "producto");

        switch (transactionType)
        {
            case "MOD":
                // Example: "Modificacion en producto {#3, Leche}"
                return $"Modificacion en producto {prodPart}";

            case "DEL":
                if (batchId.HasValue)
                    // Example: "Lote ID #69, con productos {#3, Leche} expirados eliminado"
                    return $"Lote ID #{batchId.Value}, con productos {prodPart} expirados eliminado";
                // Example: "Se elimino el producto {#3, Leche} y sus lotes"
                return $"Se elimino el producto {prodPart} y sus lotes";

            case "ADD":
                // Example: "Se añadio el producto {#3, Leche}"
                return $"Se añadio el producto {prodPart}";

            case "IN":
                if (quantity.HasValue)
                    // Example: "Se añadio a producto {#3, Leche} un nuevo lote de 50 productos"
                    return $"Se añadio a producto {prodPart} un nuevo lote de {quantity.Value} productos";
                // Example: "Se añadio a producto {#3, Leche} un nuevo lote"
                return $"Se añadio a producto {prodPart} un nuevo lote";

            case "OUT":
                if (quantity.HasValue)
                    // Example: "Se vendieron 50 productos {#3, Leche}"
                    return $"Se vendieron {quantity.Value} productos {prodPart}";
                // Example: "Se realizo una venta de {#3, Leche}"
                return $"Se realizo una venta de {prodPart}";

            default:
                return $"Operacion sobre {prodPart}";
        }
    }
    
    private static string BuildExtendedDescription(string transactionType,
        int? productId = null,
        string? productName = null,
        int? quantity = null,
        decimal? unitCost = null,
        string? reason = null,
        int? batchId = null,
        DateTime? expiration = null)
    {
        string prodPart = productId.HasValue ? $"Producto {{#{productId.Value}, {productName}}}" : (productName != null ? $"Producto {{{productName}}}" : "Producto");

        switch (transactionType)
        {
            case "MOD":
                // Detailed MOD: "Producto {#3, Leche} se modifico: (opcional)nombre, (opcional)stock minimo, (opcional)precio"
                var modReason = string.IsNullOrWhiteSpace(reason) ? string.Empty : $": {reason}";
                return $"{prodPart} se modifico{modReason}";

            case "DEL":
                // Batch delete with expiration: "Lote ID #69, con 50 productos {#3, Leche}, se elimino al estar caducados (25/12/24)"
                if (batchId.HasValue && quantity.HasValue)
                {
                    var expStr = expiration.HasValue ? $" ({expiration.Value:dd/MM/yy})" : string.Empty;
                    return $"Lote ID #{batchId.Value}, con {quantity.Value} productos {prodPart}, se elimino al estar caducados {expStr}";
                }
                // Full product delete: "Se elimino el producto {#3, Leche} y sus lotes"
                return $"Se elimino el producto {prodPart} y sus lotes";

            case "ADD":
                // Created with initial batch: "Producto {#3, Leche} fue creado con un lote inicial de 50 productos con expiracion 25/12/36"
                if (quantity.HasValue)
                {
                    var expPart = expiration.HasValue ? $" con expiracion {expiration.Value:dd/MM/yy}" : string.Empty;
                    return $"{prodPart} fue creado con un lote inicial de {quantity.Value} productos{expPart}";
                }
                return $"{prodPart} fue creado";

            case "IN":
                // New batch added: "Producto {#3, Leche} se añadio un nuevo lote #69 con 50 productos con expiracion 25/12/36"
                if (batchId.HasValue && quantity.HasValue)
                {
                    var expPart = expiration.HasValue ? $" con expiracion {expiration.Value:dd/MM/yy}" : string.Empty;
                    return $"{prodPart} se añadio un nuevo lote #{batchId.Value} con {quantity.Value} productos{expPart}";
                }
                return quantity.HasValue ? $"{prodPart} se añadio un nuevo lote con {quantity.Value} productos" : $"{prodPart} se añadio stock";

            case "OUT":
                // Sale: "Producto {#3, Leche} se realizo una venta de 50 productos con precio de $50 dando un total de $250"
                if (quantity.HasValue && unitCost.HasValue)
                {
                    var total = quantity.Value * unitCost.Value;
                    return $"{prodPart} se realizo una venta de {quantity.Value} productos con precio de ${unitCost.Value:F} dando un total de ${total:F}";
                }
                return quantity.HasValue ? $"{prodPart} se realizo una venta de {quantity.Value} productos" : $"{prodPart} se realizo una venta";

            default:
                return $"{prodPart} {reason ?? string.Empty}".Trim();
        }
    }
}