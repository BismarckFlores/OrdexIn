using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Models.DTO;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IPointOfSaleService _pointOfSaleService;
    private const int MaxPageSize = 100, MinPageSize = 10;
    
    public ProductController(IProductService productService, IPointOfSaleService pointOfSaleService)
    {
        _productService = productService;
        _pointOfSaleService = pointOfSaleService;
    }
    
    // GET: api/product?page=1&pageSize=100&lowStock=true
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = MaxPageSize, [FromQuery] bool lowStock = false)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, MinPageSize, MaxPageSize);

        var all = await _productService.GetAllProductsAsync();

        if (lowStock)
        {
            all = all.Where(p => p.Stock < p.StockMin).ToList();
        }

        var total = all.Count;
        var items = all
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var itemsDto = items.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                MinStock = p.StockMin
            }
        ).ToList();

        return Ok(new { page, pageSize, total, items = itemsDto });
    }

    // GET: api/product/search?q=term&page=1&pageSize=100&lowStock=true
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1,
        [FromQuery] int pageSize = MaxPageSize, [FromQuery] bool lowStock = false)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, MinPageSize, MaxPageSize);

        var all = await _productService.GetAllProductsAsync();

        var filtered = all
            .Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(q ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (lowStock)
        {
            filtered = filtered.Where(p => p.Stock < p.StockMin).ToList();
        }

        var total = filtered.Count;
        var items = filtered
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    MinStock = p.StockMin
                }
            ).ToList();

        return Ok(new { page, pageSize, total, items });
    }
    
    // GET: api/product/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _productService.GetByIdAsync(id);
        if (p == null) return NotFound("Product not found");

        var dto = new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock,
            MinStock = p.StockMin
        };

        return Ok(dto);
    }
    
    // POST: api/product
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
    {
        var userId = ObtainUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized("User ID not found in claims");
        
        var product = new ProductModel
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            StockMin = dto.MinStock,
            CreatedAt = DateTime.UtcNow
        };
        
        var createdProduct = await _productService.CreateAsync(product, userId);
        if (createdProduct == null)
            return StatusCode(500, "Failed to add product");
        
        var batch = new LotModel
        {
            ProductId = createdProduct.Id,
            Quantity = dto.Stock,
            ExpirationDate = dto.ExpirationDate,
            CreatedAt = DateTime.UtcNow
        };
        
        var okBatch = await _pointOfSaleService.AddNewBatchAsync(createdProduct, batch, userId, true);
        if (!okBatch)
        {
            await _productService.RemoveAsync(product.Id, userId, true);
            return StatusCode(500, "Failed to add batch");
        }
        
        return Ok( new {success = true});
    }
    
    // PUT: api/product/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto)
    {
        var userId = ObtainUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized("User ID not found in claims");
        
        var ok = await _productService.UpdateAsync(dto, userId);
        return ok ? Ok( new {success = true}) : StatusCode(500, "Failed to update product");
    }
    
    // DELETE: api/product/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = ObtainUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized("User ID not found in claims");
        
        var ok = await _productService.RemoveAsync(id, userId);
        return ok ? Ok( new {success = true}) : StatusCode(500, "Failed to delete product");
    }

    // POST: api/product/5/batch
    [HttpPost("{id:int}/batch")]
    public async Task<IActionResult> AddBatch(int id, [FromBody] BatchCreateDto dto)
    {
        var prod = await _productService.GetByIdAsync(id);
        if (prod == null) return NotFound("Product not found");
        
        var userId = ObtainUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized("User ID not found in claims");
        
        var batch = new LotModel
        {
            ProductId = id,
            Quantity = dto.Quantity,
            ExpirationDate = dto.ExpirationDate,
            CreatedAt = DateTime.UtcNow
        };
        
        var ok = await _pointOfSaleService.AddNewBatchAsync(prod, batch, userId);
        return ok ? Ok(new { success = true }) : StatusCode(500, "Failed to add batch");
    }
    
    // POST: api/product/5/sell
    [HttpPost("{id:int}/sell")]
    public async Task<IActionResult> SellProduct(int id, [FromBody] SellEntryDto entryDto)
    {
        var prod = await _productService.GetByIdAsync(id);
        if (prod == null) return NotFound("Product not found");
        
        if (entryDto.Quantity <= 0) return BadRequest("Invalid quantity");

        var userId = ObtainUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized("User ID not found in claims");
        
        var ok = await _pointOfSaleService.RegisterSaleAsync(prod, entryDto.Quantity, userId);
        return ok ? Ok(new { success = true }) : StatusCode(500, "Failed to register sale");
    }
    
    // Helper functions
    private Guid ObtainUserIdFromClaims()
    {
        var userId = Guid.Empty;
        var claimKeys = new[] { ClaimTypes.NameIdentifier, "sub", "user_id", "UserId", "uid", "id" };
        foreach (var claimKey in claimKeys)
        {
            var value = User.FindFirst(claimKey)?.Value;
            if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var g))
            {
                userId = g;
                break;
            }
        }
    
        return userId;
    }
}