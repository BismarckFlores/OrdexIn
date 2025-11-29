using OrdexIn.Models;

namespace OrdexIn.Services.Intefaces;

public interface IProductService
{
    // Obtener todos los productos
    Task<List<Product>> GetAllProductsAsync();
    
    // Obtener un producto por ID
    Task<Product?> GetByIdAsync(int id);
    
    // Crear un nuevo producto
    Task<bool> CreateAsync(Product item);
    
    // Actualizar un producto existente
    Task<bool> UpdateAsync(Product item);
    
    // Eliminar un producto por ID
    Task<bool> RemoveAsync(int id);
    
    // Verificar si un producto existe por ID
    Task<bool> ExistsAsync(int id);
    
    // Contar el total de productos
    Task<int> GetTotalInventoryAsync();
}