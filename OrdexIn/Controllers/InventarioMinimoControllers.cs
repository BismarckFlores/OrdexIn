using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Services;

namespace OrdexIn.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioMinimoController : ControllerBase
    {
        private readonly InventarioMinimoDao _dao;

        public InventarioMinimoController(InventarioMinimoDao dao)
        {
            _dao = dao;
        }

        // ✔ GET: Todos los registros
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var data = await _dao.ObtenerTodos();
            return Ok(data);
        }

        // ✔ GET: Buscar por ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var data = await _dao.ObtenerPorId(id);

            if (data == null)
                return NotFound(new { mensaje = "Inventario mínimo no encontrado." });

            return Ok(data);
        }

        // ✔ GET: Buscar por producto
        [HttpGet("producto/{idProducto:int}")]
        public async Task<IActionResult> ObtenerPorProducto(int idProducto)
        {
            var data = await _dao.ObtenerPorProducto(idProducto);

            if (data == null)
                return NotFound(new { mensaje = "Este producto no tiene inventario mínimo configurado." });

            return Ok(data);
        }

        // ✔ POST: Insertar un registro
        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] InventarioMinimo item)
        {
            var nuevo = await _dao.Insertar(item);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo?.IdInventarioMinimo }, nuevo);
        }

        // ✔ PUT: Actualizar registro
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] InventarioMinimo item)
        {
            var existente = await _dao.ObtenerPorId(id);

            if (existente == null)
                return NotFound(new { mensaje = "El registro que intentas actualizar no existe." });

            item.IdInventarioMinimo = id;

            await _dao.Actualizar(item);

            return Ok(new { mensaje = "Inventario mínimo actualizado correctamente." });
        }

        // ✔ DELETE: Eliminar registro
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var existente = await _dao.ObtenerPorId(id);

            if (existente == null)
                return NotFound(new { mensaje = "El registro que intentas eliminar no existe." });

            await _dao.Eliminar(id);

            return Ok(new { mensaje = "Inventario mínimo eliminado correctamente." });
        }

        // ✔ GET: Calcular inventario mínimo recomendado
        [HttpGet("calcular")]
        public IActionResult CalcularInventarioMinimo([FromQuery] int ventasPromedio)
        {
            int minimo = _dao.CalcularInventarioMinimo(ventasPromedio);

            return Ok(new
            {
                ventasPromedio,
                inventarioMinimoRecomendado = minimo
            });
        }
    }
}

