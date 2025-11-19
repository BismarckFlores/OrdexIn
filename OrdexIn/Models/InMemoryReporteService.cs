using System;
using System.Collections.Generic;
using System.Linq;
using OrdexIn.Models;

namespace OrdexIn.Services
{
    public class InMemoryReporteService : IReporteService
    {
        private readonly List<VentaModel> _ventas;

        public InMemoryReporteService()
        {
            // datos de ejemplo; en producción obtén de DB/Supabase
            _ventas = new List<VentaModel>
            {
                new VentaModel { IdVenta = 1, Fecha = DateTime.UtcNow.AddDays(-10), Total = 120.50m, Usuario = "cajero1" },
                new VentaModel { IdVenta = 2, Fecha = DateTime.UtcNow.AddDays(-7), Total = 75.00m, Usuario = "cajero2" },
                new VentaModel { IdVenta = 3, Fecha = DateTime.UtcNow.AddDays(-3), Total = 200m, Usuario = "cajero1" }
            };
        }

        public IEnumerable<VentaModel> ObtenerVentas(DateTime desde, DateTime hasta)
        {
            return _ventas.Where(v => v.Fecha >= desde && v.Fecha <= hasta).OrderBy(v => v.Fecha);
        }

        public IEnumerable<(DateTime Fecha, decimal Total)> VentasPorPeriodo(DateTime desde, DateTime hasta)
        {
            return _ventas
                .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                .GroupBy(v => v.Fecha.Date)
                .OrderBy(g => g.Key)
                .Select(g => (g.Key, g.Sum(v => v.Total)));
        }
    }
}