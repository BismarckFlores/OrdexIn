using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PointOfSaleController : ControllerBase
{
    private readonly IPointOfSaleService _ps;
    
    public PointOfSaleController(IPointOfSaleService ps)
    {
        _ps = ps;
    }
    
    // GET: api/pointofsale/inventory/5
    [HttpGet("inventory/{productId:int}")]
    public async Task<IActionResult> Get(int productId)
    {
        var batches = await _ps.GetInventoryAsync(productId);
        var dtoBatches = batches.Select(b => new LotDto
            {
                Id =  b.Id,
                ProductId = b.ProductId,
                Quantity = b.Quantity,
                ExpirationDate = b.ExpirationDate
            }
        ).ToList();
        
        return Ok(dtoBatches);
    }
}