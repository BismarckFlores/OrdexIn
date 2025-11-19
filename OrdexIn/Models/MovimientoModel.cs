using System;

namespace OrdexIn.Models
{
    public enum TipoMovimiento
    {
        Entrada = 1,
        Salida = 2,
        Ajuste = 3
    }

    public class MovimientoModel
    {
        public int IdMovimiento { get; set; }
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public TipoMovimiento Tipo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal SaldoResultante { get; set; } // calculado por el servicio
        public string? Usuario { get; set; }
        public string? Observacion { get; set; }
    }
}