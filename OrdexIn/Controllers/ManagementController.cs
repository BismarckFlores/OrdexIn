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

        public ActionResult Index() => View();
        
        public ActionResult Users() => View();
    }
}
