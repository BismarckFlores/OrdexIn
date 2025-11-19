using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrdexIn.Models;

namespace OrdexIn.Pages.Categorias
{
    public class Create : PageModel
    {
        [BindProperty]
        public CategoriaModel Categoria { get; set; } = new();

        // Eliminar este método duplicado si ya existe otro OnPost en la clase
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // Aquí deberías guardar la categoría en la base de datos
            // Por ahora, redirige a Index
            return RedirectToPage("Index");
        }
    }
}