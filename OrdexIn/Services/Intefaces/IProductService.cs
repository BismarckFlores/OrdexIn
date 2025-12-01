using OrdexIn.Models;
using OrdexIn.Models.DTO;

namespace OrdexIn.Services.Intefaces;

public interface IProductService
{
    // Obtener todos los productos
    Task<List<ProductModel>> GetAllProductsAsync();
    
    // Obtener un producto por ID
    Task<ProductModel?> GetByIdAsync(int id);
    
    // Crear un nuevo producto
    Task<ProductModel?> CreateAsync(ProductModel item, Guid userId);
    
    // Actualizar un producto existente
    Task<bool> UpdateAsync(ProductDto item, Guid userId);
    
    // Eliminar un producto por ID
    Task<bool> RemoveAsync(int id, Guid userId, bool isRollback = false);
    
    // Verificar si un producto existe por ID
    Task<bool> ExistsAsync(int id);
    
    // Contar el total de productos
    Task<int> GetTotalInventoryAsync();
}