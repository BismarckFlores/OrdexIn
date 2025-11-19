using System;
using System.Collections.Generic;
using OrdexIn.Models;

namespace OrdexIn.Services
{
    public class InMemoryAuditService : IAuditService
    {
        private readonly List<AuditLogModel> _logs = new();

        public void Registrar(AuditLogModel log)
        {
            log.Id = _logs.Count > 0 ? _logs.Max(l => l.Id) + 1 : 1;
            log.Fecha = log.Fecha == default ? DateTime.UtcNow : log.Fecha;
            _logs.Add(log);
            // En producción: guardar en tabla AuditLogs o enviar a sistema de logs centralizado
        }
    }
}