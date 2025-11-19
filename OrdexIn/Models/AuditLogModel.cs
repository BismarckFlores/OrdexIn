using System;

namespace OrdexIn.Models
{
    public class AuditLogModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty; // e.g. "Crear", "Editar", "Eliminar", "Movimiento"
        public string Entidad { get; set; } = string.Empty; // e.g. "Producto", "Categoria"
        public string EntidadId { get; set; } = string.Empty;
        public string? Detalles { get; set; }
    }
}
