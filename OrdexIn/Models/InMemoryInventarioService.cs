using System;
using System.Collections.Generic;
using System.Linq;
using OrdexIn.Models;
using System.Threading;

namespace OrdexIn.Services
{
    // Implementación simple en memoria para prototipado
    public class InMemoryInventarioService : IInventarioService
    {
        private readonly List<MovimientoModel> _movimientos = new();
        private readonly ReaderWriterLockSlim _lock = new();

        public IEnumerable<MovimientoModel> GetMovimientos(int idProducto, DateTime? desde = null, DateTime? hasta = null)
        {
            _lock.EnterReadLock();
            try
            {
                var q = _movimientos.Where(m => m.IdProducto == idProducto);
                if (desde.HasValue) q = q.Where(m => m.Fecha >= desde.Value);
                if (hasta.HasValue) q = q.Where(m => m.Fecha <= hasta.Value);
                return q.OrderBy(m => m.Fecha).ThenBy(m => m.IdMovimiento).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public MovimientoModel RegistrarMovimiento(MovimientoModel movimiento)
        {
            _lock.EnterWriteLock();
            try
            {
                movimiento.IdMovimiento = _movimientos.Count > 0 ? _movimientos.Max(m => m.IdMovimiento) + 1 : 1;
                movimiento.Fecha = movimiento.Fecha == default ? DateTime.UtcNow : movimiento.Fecha;

                // calcular saldo resultante tomando el último saldo conocido
                var ultimo = _movimientos.LastOrDefault(m => m.IdProducto == movimiento.IdProducto);
                var saldoPrev = ultimo?.SaldoResultante ?? 0m;
                var delta = movimiento.Tipo == TipoMovimiento.Entrada || movimiento.Tipo == TipoMovimiento.Ajuste
                    ? movimiento.Cantidad
                    : -movimiento.Cantidad;

                movimiento.SaldoResultante = saldoPrev + delta;

                _movimientos.Add(movimiento);
                return movimiento;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public decimal ObtenerSaldoActual(int idProducto)
        {
            _lock.EnterReadLock();
            try
            {
                var ultimo = _movimientos.LastOrDefault(m => m.IdProducto == idProducto);
                return ultimo?.SaldoResultante ?? 0m;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}