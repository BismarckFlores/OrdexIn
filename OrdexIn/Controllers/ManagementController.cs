using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Controllers
{
    [Authorize(Roles = "admin")]
    public class ManagementController : Controller
    {
        private readonly IKardexDataService _kardexService;

        public ManagementController(IKardexDataService kardexService)
        {
            _kardexService = kardexService;
        }

        // GET: KardexController
        public ActionResult Index()
        {
            return View();
        }
    }
}
