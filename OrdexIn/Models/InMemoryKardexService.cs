using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrdexIn.Models;

namespace OrdexIn.Services
{
    public class InMemoryKardexService : IKardexService
    {
        // listas en memoria (thread-safe con lock)
        private readonly List<KardexEntryModel> _movimientos = new();
        private readonly List<string> _auditLogs = new();
        private readonly object _lock = new();

        public Task AddAuditLogAsync(string mensaje)
        {
            lock (_lock)
            {
                _auditLogs.Add($"[{DateTime.UtcNow:O}] {mensaje}");
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetAuditLogsAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_auditLogs.AsEnumerable());
            }
        }

        public Task AddMovimientoAsync(KardexEntryModel movimiento)
        {
            if (movimiento == null) throw new ArgumentNullException(nameof(movimiento));

            lock (_lock)
            {
                // Añadir el movimiento a la colección
                _movimientos.Add(movimiento);

                // Recalcular saldos para el producto afectado (por fecha asc)
                var productoMovs = _movimientos
                    .Where(m => m.ProductoId == movimiento.ProductoId)
                    .OrderBy(m => m.Fecha)
                    .ToList();

                decimal running = 0m;
                foreach (var m in productoMovs)
                {
                    // Determinar si es ingreso o salida por texto (ajusta según tu dominio)
                    bool esIngreso = !string.IsNullOrEmpty(m.Tipo) &&
                                     (m.Tipo.Contains("ingreso", StringComparison.OrdinalIgnoreCase) ||
                                      m.Tipo.Contains("entrada", StringComparison.OrdinalIgnoreCase) ||
                                      m.Tipo.Contains("compra", StringComparison.OrdinalIgnoreCase));

                    running += esIngreso ? m.Cantidad : -m.Cantidad;
                    m.SaldoResultante = running;
                }

                // Añadir entrada a logs de auditoría
                _auditLogs.Add($"[{DateTime.UtcNow:O}] Movimiento añadido: ProductoId={movimiento.ProductoId}, Tipo={movimiento.Tipo}, Cantidad={movimiento.Cantidad}, Usuario={movimiento.Usuario}");
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<KardexEntryModel>> GetAllMovimientosAsync()
        {
            lock (_lock)
            {
                // devolver copia ordenada por fecha
                return Task.FromResult(_movimientos.OrderBy(m => m.Fecha).ToList().AsEnumerable());
            }
        }

        public Task<IEnumerable<KardexEntryModel>> GetKardexAsync(int productoId, DateTime? desde = null, DateTime? hasta = null)
        {
            lock (_lock)
            {
                var q = _movimientos.Where(m => m.ProductoId == productoId);

                if (desde.HasValue)
                    q = q.Where(m => m.Fecha >= desde.Value);
                if (hasta.HasValue)
                    q = q.Where(m => m.Fecha <= hasta.Value);

                // devolver copia ordenada por fecha asc
                var result = q.OrderBy(m => m.Fecha).ToList();

                // Aseguramos que SaldoResultante esté calculado; si no lo está, recalculamos localmente
                if (result.Count > 0 && result.Any(m => m.SaldoResultante == 0))
                {
                    decimal running = 0m;
                    // Necesitamos calcular el saldo inicial: buscar el saldo anterior al primer registro filtrado
                    var previous = _movimientos
                        .Where(m => m.ProductoId == productoId && m.Fecha < result.First().Fecha)
                        .OrderBy(m => m.Fecha);

                    foreach (var p in previous)
                    {
                        bool pIngreso = !string.IsNullOrEmpty(p.Tipo) &&
                                        (p.Tipo.Contains("ingreso", StringComparison.OrdinalIgnoreCase) ||
                                         p.Tipo.Contains("entrada", StringComparison.OrdinalIgnoreCase) ||
                                         p.Tipo.Contains("compra", StringComparison.OrdinalIgnoreCase));
                        running += pIngreso ? p.Cantidad : -p.Cantidad;
                    }

                    // aplicar sobre registros filtrados
                    foreach (var r in result)
                    {
                        bool rIngreso = !string.IsNullOrEmpty(r.Tipo) &&
                                        (r.Tipo.Contains("ingreso", StringComparison.OrdinalIgnoreCase) ||
                                         r.Tipo.Contains("entrada", StringComparison.OrdinalIgnoreCase) ||
                                         r.Tipo.Contains("compra", StringComparison.OrdinalIgnoreCase));
                        running += rIngreso ? r.Cantidad : -r.Cantidad;
                        r.SaldoResultante = running;
                    }
                }

                return Task.FromResult(result.AsEnumerable());
            }
        }
    }
}
