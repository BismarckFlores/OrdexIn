using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrdexIn.Models;

namespace OrdexIn.Pages.Categorias
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public CategoriaModel Categoria { get; set; } = new();

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