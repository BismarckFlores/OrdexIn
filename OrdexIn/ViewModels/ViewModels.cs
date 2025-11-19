using OrdexIn.Models;

namespace OrdexIn.ViewModels
{
    public class ProductoFormViewModel
    {
        public ProductosModels Producto { get; set; }
        public IEnumerable<CategoriaModel> Categorias { get; set; }
    }

}
