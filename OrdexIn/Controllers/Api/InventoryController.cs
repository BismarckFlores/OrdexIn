using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
   private readonly IProductService _productService;
   
   public InventoryController(IProductService productService)
   {
       _productService = productService;
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
}