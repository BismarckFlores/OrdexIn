using System;
using System.Collections.Generic;
using OrdexIn.Models;

namespace OrdexIn.Services
{
    public interface IReporteService
    {
        IEnumerable<(DateTime Fecha, decimal Total)> VentasPorPeriodo(DateTime desde, DateTime hasta);
        IEnumerable<VentaModel> ObtenerVentas(DateTime desde, DateTime hasta);
    }
}
