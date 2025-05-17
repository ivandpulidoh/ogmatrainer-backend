using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MembershipService.Models
{
    public enum EstadoPago
    {
        Pendiente,
        Completado,
        Fallido,
        Reembolsado
    }

    [Table("Pagos")]
    public class Pago
    {
        [Key]
        [Column("id_pago")]
        public int IdPago { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_membresia")]
        public int? IdMembresia { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }

        [StringLength(3)]
        public string Moneda { get; set; } = "USD";

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        [Column("metodo_pago")]
        public string? MetodoPago { get; set; } // e.g., 'PayPal', 'TarjetaCredito'

        [StringLength(100)]
        [Column("id_transaccion_externa")]
        public string? IdTransaccionExterna { get; set; }

        [Required]
        [StringLength(15)]
        [Column("estado_pago")]
        public string EstadoPago { get; set; } = Models.EstadoPago.Pendiente.ToString();

        [Column(TypeName = "NVARCHAR(MAX)")]
        public string? Descripcion { get; set; }
        
        [ForeignKey("IdMembresia")]
        public virtual Membresia? Membresia { get; set; }
       
    }
}