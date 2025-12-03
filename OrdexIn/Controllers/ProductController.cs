using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models.DTO;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        
        // GET: ProductController
        public ActionResult Index() => View();

        // GET: /Product/Details/5
        public ActionResult Details(int id)
        {
            var product = _productService.GetByIdAsync(id).Result;
            if (product == null) return NotFound("Product not found");
            
            var vm = new ProductDetailsDto()
            {
                ProductModel = product
            };
            
            return View(vm);
        }
    }
}
