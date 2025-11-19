namespace OrdexIn.Models
{
    public class KardexEntryModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int ProductoId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string? Tipo { get; set; } // e.g. "Ingreso", "Salida", "Ajuste"
        public decimal Cantidad { get; set; }
        public string? Usuario { get; set; }
        public string? Observacion { get; set; }

        // Saldo resultante después de aplicar este movimiento (se calcula por el servicio)
        public decimal SaldoResultante { get; set; }
    }
}
