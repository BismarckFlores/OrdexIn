using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
   private readonly IProductService _productService;
   private readonly IKardexDataService _kardexService;
   
   public InventoryController(IProductService productService, IKardexDataService kardexService)
   {
       _productService = productService;
         _kardexService = kardexService;
   }
   
   [HttpGet("stats")]
   public async Task<ActionResult<InventoryStats>> GetStats()
   {
       // compute minimal stats quickly (use service methods you already have)
       var allProducts = await _productService.GetAllProductsAsync();
       var stats = new InventoryStats
       {
           TotalProducts = allProducts.Count,
           TotalStock = allProducts.Sum(p => p.Stock),
           TotalInventoryValue = allProducts.Sum(p => p.Stock * p.Price),
           LowStockCount = allProducts.Count(p => p.Stock < p.StockMin), // threshold example
           LastUpdatedUtc = DateTime.UtcNow
       };
       return Ok(stats);
   }

   [HttpGet("recent")]
   public async Task<IActionResult> GetRecentMovemnts()
   {
       var all = await _kardexService.GetAllKardexEntriesAsync();
       var recent = all
           .OrderByDescending(k => k.CreatedAt)
           .Take(10)
           .Select(k => new RecentMovements
           {
               TransactionType = k.TransactionType,
               Quantity = k.Quantity,
               Price = k.UnitCost,
               Total = k.TotalCost,
               Reason = k.Reason,
               CreatedAt = k.CreatedAt
           });
       
         return Ok(recent);
   }
}