namespace MembershipService.DTOs
{
    public class PagoDto
    {
        public int IdPago { get; set; }
        public int IdUsuario { get; set; }
        public int? IdMembresia { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public string? MetodoPago { get; set; }
        public string? IdTransaccionExterna { get; set; }
        public string EstadoPago { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class CreatePagoDto
    {
        public int IdUsuario { get; set; }
        public int IdMembresia { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "COP";
        public string MetodoPago { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? EstadoPago { get; set; }        
    }
   
    public class UpdatePagoStatusDto
    {
        public string EstadoPago { get; set; } = string.Empty; // Solo para actualizar el estado
        public string? IdTransaccionExterna { get; set; } // Si se obtiene después
    }
}