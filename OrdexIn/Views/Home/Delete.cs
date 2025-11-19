using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrdexIn.Models;

namespace OrdexIn.Pages.Categorias
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public CategoriaModel Categoria { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            // Aquí deberías obtener la categoría por id desde la base de datos
            Categoria = new CategoriaModel { IdCategoria = id, NombreCategoria = "Ejemplo", Descripcion = "Descripción" };
            if (Categoria == null)
                return NotFound();

            return Page();
        }

        public IActionResult OnPost()
        {
            // Aquí deberías eliminar la categoría de la base de datos
            return RedirectToPage("Index");
        }
    }
}
