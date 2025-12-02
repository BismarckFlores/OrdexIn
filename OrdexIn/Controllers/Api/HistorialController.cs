using Microsoft.AspNetCore.Mvc;
using OrdexIn.Services.Intefaces;
using OrdexIn.Models;
using OrdexIn.Models.DTO;
using OrdexIn.Services;

namespace OrdexIn.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class HistorialController : ControllerBase
{
    private readonly IKardexDataService _kardexService;
    private readonly IAuthService _authService;
    private const int MaxPageSize = 100, MinPageSize = 10;

    public HistorialController(IKardexDataService k, IAuthService auth)
    {
        _kardexService = k;
        _authService = auth;
    }

    // GET: api/manager?page=1&pageSize=100
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = MaxPageSize)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, MinPageSize, MaxPageSize);
        
        var all = await _kardexService.GetAllKardexEntriesAsync();
        
        var total = all.Count;
        var items = all
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(k => new KardexDisplayDto
                {
                    KardexId = k.Id,
                    Type = k.TransactionType,
                    Description = k.ExtendedDescription ?? k.Reason,
                    Quantity = k.Quantity,
                    UnitPrice = k.UnitCost,
                    Total = k.TotalCost,
                    UserEmail = _authService.GetUserEmailAsync(k.UserId).Result,
                    CreatedAt = k.CreatedAt
                }
            );

        return Ok(new { page, pageSize, total, items });
    }
    
    // GET: api/manager/search?initialDate=2024-01-01&finalDate=2024-01-31&page=1&pageSize=100
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] DateTime initialDate, [FromQuery] DateTime finalDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = MaxPageSize)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, MinPageSize, MaxPageSize);
        
        var all = await _kardexService.GetAllKardexEntriesAsync();
        var filtered = all
            .Where(k => k.CreatedAt.Date >= initialDate.Date && k.CreatedAt.Date <= finalDate.Date)
            .ToList();
        
        var total = filtered.Count;
        var items = filtered
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(k => new KardexDisplayDto
                {
                    KardexId = k.Id,
                    Type = k.TransactionType,
                    Description = k.ExtendedDescription ?? k.Reason,
                    Quantity = k.Quantity,
                    UnitPrice = k.UnitCost,
                    Total = k.TotalCost,
                    UserEmail = _authService.GetUserEmailAsync(k.UserId).Result,
                    CreatedAt = k.CreatedAt
                }
            );
        
        return Ok(new { page, pageSize, total, items });
    }
}