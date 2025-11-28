using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrdexIn.Controllers
{
    public class PuntoVentaController : Controller
    {
        // GET: PuntoVentaController
        public ActionResult Index()
        {
            return View();
        }

        // GET: PuntoVentaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PuntoVentaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PuntoVentaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PuntoVentaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PuntoVentaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PuntoVentaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PuntoVentaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
