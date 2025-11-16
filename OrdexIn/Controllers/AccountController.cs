using Microsoft.AspNetCore.Mvc;

namespace OrdexIn.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
