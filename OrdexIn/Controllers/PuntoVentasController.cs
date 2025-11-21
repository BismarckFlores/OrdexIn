using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Services;
using OrdexIn.Services.OrdexIn.Services;

namespace OrdexIn.Controllers
{
    [Authorize]
    public class PuntoVentasController : Controller
    {
        private readonly PuntoVentaDao _puntoVentaDao;
        private readonly ProductDao _productDao;

        public PuntoVentasController(PuntoVentaDao puntoVentaDao, ProductDao productDao)
        {
            _puntoVentaDao = puntoVentaDao;
            _productDao = productDao;
        }

        // ==========================================================
        // INDEX – Visualización general del módulo Punto de Venta
        // ==========================================================
        public async Task<ActionResult> Index()
        {
            try
            {
                var productos = await _productDao.GetAll();
                return View(productos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error cargando los productos: " + ex.Message;
                return View(new List<ProductModel>());
            }
        }

        // ==========================================================
        // DETALLES – Ver inventario FIFO del producto
        // ==========================================================
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var producto = await _productDao.GetForId(id);
                if (producto == null)
                    return NotFound();

                var inventario = await _puntoVentaDao.GetInventoryFIFO(id);

                ViewBag.Producto = producto;
                return View(inventario);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error cargando detalles del producto: " + ex.Message;
                return View();
            }
        }

        // ==========================================================
        // REALIZAR VENTA – Formulario
        // ==========================================================
        public async Task<ActionResult> Create()
        {
            var productos = await _productDao.GetAll();
            return View(productos);
        }

        // ==========================================================
        // REALIZAR VENTA – Procesamiento
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int idProducto, int cantidad)
        {
            try
            {
                if (cantidad <= 0)
                {
                    ViewBag.Error = "La cantidad debe ser mayor a 0.";
                    return View(await _productDao.GetAll());
                }

                var resultado = await _puntoVentaDao.RegisterSale(idProducto, cantidad);

                ViewBag.Mensaje = resultado;

                return RedirectToAction(nameof(Details), new { id = idProducto });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error procesando la venta: " + ex.Message;
                return View(await _productDao.GetAll());
            }
        }

        // ==========================================================
        // EDIT – (No aplica directamente para ventas, pero se deja base)
        // ==========================================================
        public async Task<ActionResult> Edit(int id)
        {
            var producto = await _productDao.GetForId(id);
            if (producto == null)
                return NotFound();

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, ProductModel producto)
        {
            try
            {
                if (id != producto.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(producto);

                var actualizado = await _productDao.Update(producto);

                if (!actualizado)
                {
                    ViewBag.Error = "No se pudo actualizar el producto.";
                    return View(producto);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error actualizando el producto: " + ex.Message;
                return View(producto);
            }
        }

        // ==========================================================
        // DELETE – (No aplica directamente; se deja por compatibilidad)
        // ==========================================================
        public async Task<ActionResult> Delete(int id)
        {
            var producto = await _productDao.GetForId(id);
            if (producto == null)
                return NotFound();

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, ProductModel producto)
        {
            try
            {
                var eliminado = await _productDao.Delete(id);

                if (!eliminado)
                {
                    ViewBag.Error = "No se pudo eliminar el producto.";
                    return View(producto);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error eliminando el producto: " + ex.Message;
                return View(producto);
            }
        }
    }
}
