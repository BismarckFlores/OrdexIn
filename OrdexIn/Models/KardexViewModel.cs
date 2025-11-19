using OrdexIn.Models;
using System;
using System.Collections.Generic;

namespace OrdexIn.Models.ViewModels
{
    public class KardexViewModel
    {
        // Filtros
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        // Filtro por producto
        public int? IdProducto { get; set; }

        // Datos reales
        public List<MovimientoModel> Movimientos { get; set; } = new();
        public List<ProductosModels> Productos { get; set; } = new();
        public List<VentaModel> Ventas { get; set; } = new();

        // Resumen financiero
        public decimal TotalVentas => Ventas?.Sum(v => v.Total) ?? 0;
        public int TotalTransacciones => Ventas?.Count ?? 0;
    }
}

