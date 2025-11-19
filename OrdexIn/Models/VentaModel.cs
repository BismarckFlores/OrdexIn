using System;

namespace OrdexIn.Models
{
    public class VentaModel
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string? Usuario { get; set; }
        public string? Detalles { get; set; } // JSON u otra estructura resumida
    }
}
