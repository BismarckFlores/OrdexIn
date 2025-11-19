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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _dao.ObtenerTodos();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _dao.ObtenerPorId(id);
            return item != null ? Ok(item) : NotFound();
        }

        [HttpGet("producto/{idProducto}")]
        public async Task<IActionResult> GetByProducto(int idProducto)
        {
            var lista = await _dao.ObtenerPorProducto(idProducto);
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] InventarioMinimo item)
        {
            var creado = await _dao.Insertar(item);
            return Ok(creado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] InventarioMinimo item)
        {
            if (id != item.IdInventario)
                return BadRequest("El ID no coincide.");

            await _dao.Actualizar(item);
            return Ok("Actualizado correctamente.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _dao.Eliminar(id);
            return Ok("Eliminado correctamente.");
        }
    }
}

