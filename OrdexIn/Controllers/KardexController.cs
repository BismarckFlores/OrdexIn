using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Models.ViewModels;
using OrdexIn.Services;

namespace OrdexIn.Controllers
{
    public class KardexController : Controller
    {
        private readonly IInventarioService _inventarioService;
        private readonly IReporteService _reporteService;

        public KardexController(
            IInventarioService inventarioService,
            IReporteService reporteService)
        {
            _inventarioService = inventarioService;
            _reporteService = reporteService;
        }

        public IActionResult Index(int? idProducto, DateTime? inicio, DateTime? fin)
        {
            var vm = new KardexViewModel
            {
                IdProducto = idProducto,
                FechaInicio = inicio,
                FechaFin = fin
            };

            // Cargar productos
            vm.Productos = _inventarioService
                           .GetMovimientos(0) // truco temporal
                           .Select(m => m.IdProducto)
                           .Distinct()
                           .Select(id => new ProductosModels
                           {
                               IdProducto = id,
                               NombreProducto = $"Producto {id}",
                               Descripcion = "Sin descripción"
                           })
                           .ToList();

            if (idProducto.HasValue)
            {
                // Cargar movimientos del kardex
                vm.Movimientos = _inventarioService
                                    .GetMovimientos(idProducto.Value, inicio, fin)
                                    .ToList();

                // Cargar ventas del periodo
                if (inicio.HasValue && fin.HasValue)
                {
                    vm.Ventas = _reporteService
                                .ObtenerVentas(inicio.Value, fin.Value)
                                .ToList();
                }
            }

            return View("Kardex", vm);
        }
    }
}
