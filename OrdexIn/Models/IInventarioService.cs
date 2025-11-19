using System;
using System.Collections.Generic;
using OrdexIn.Models;

namespace OrdexIn.Services
{
    public interface IInventarioService
    {
        IEnumerable<MovimientoModel> GetMovimientos(int idProducto, DateTime? desde = null, DateTime? hasta = null);
        MovimientoModel RegistrarMovimiento(MovimientoModel movimiento);
        decimal ObtenerSaldoActual(int idProducto);
    }
}
