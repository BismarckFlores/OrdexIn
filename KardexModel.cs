OrdexIn/Pages/Reportes/Kardex.cshtml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OrdexIn.Models;

namespace OrdexIn.Pages.Reportes
{
    public class KardexModel : PageModel
    {
        private readonly ILogger<KardexModel> _logger;
        private readonly IReporteService? _reporteService;

        public KardexModel(ILogger<KardexModel> logger, IReporteService? reporteService = null)
        {
            _logger = logger;
            _reporteService = reporteService;
        }

        [FromRoute(Name = "idProducto")]
        public int IdProducto { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Desde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Hasta { get; set; }

        public List<KardexRow> Movimientos { get; set; } = new();

        public decimal SaldoActual { get; set; }

        public async Task<IActionResult> OnGetAsync(int idProducto)
        {
            IdProducto = idProducto;

            IEnumerable<VentaModel> ventas = Enumerable.Empty<VentaModel>();

            try
            {
                if (_reporteService is not null)
                {
                    // Llamada directa a la API disponible: ObtenerVentas(desde, hasta)
                    // Ajusta si tu método es asincrónico.
                    ventas = _reporteService.ObtenerVentas(Desde ?? DateTime.MinValue, Hasta ?? DateTime.MaxValue) ?? Enumerable.Empty<VentaModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener ventas desde IReporteService. Se continuará con lista vacía.");
            }

            // Ordenar por fecha ascendente para cálculo de saldo acumulado
            var ordered = ventas.OrderBy(v => v.Fecha).ToList();

            decimal running = 0m;
            var rows = new List<KardexRow>();

            foreach (var v in ordered)
            {
                decimal cantidad = v.Total;
                // Como ejemplo se marca tipo "Venta"; si hay diferentes tipos ajusta aquí
                string tipo = "Venta";

                // Acumular saldo (aquí se suma el total de ventas). Si necesita restar para representar stock, invierta la operación.
                running += cantidad;

                rows.Add(new KardexRow
                {
                    Fecha = v.Fecha,
                    Tipo = tipo,
                    Cantidad = cantidad,
                    SaldoResultante = running,
                    Usuario = v.Usuario,
                    Observacion = v.Detalles
                });
            }

            Movimientos = rows;
            SaldoActual = running;

            return Page();
        }

        public class KardexRow
        {
            public DateTime Fecha { get; set; }
            public string? Tipo { get; set; }
            public decimal Cantidad { get; set; }
            public decimal SaldoResultante { get; set; }
            public string? Usuario { get; set; }
            public string? Observacion { get; set; }
        }
    }
}