using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MembershipService.Models
{
    public enum EstadoMembresia
    {
        Activa,
        Inactiva,
        Expirada,
        PendientePago,
        Cancelada
    }

    [Table("Membresias")]
    public class Membresia
    {
        [Key]
        [Column("id_membresia")]
        public int IdMembresia { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; } // Asumimos que la tabla Usuarios existe y este es el FK

        [Column("id_tipo_membresia")]
        public int IdTipoMembresia { get; set; }

        [Column("id_gimnasio_principal")]
        public int? IdGimnasioPrincipal { get; set; } // Asumimos que la tabla Gimnasios existe

        [Column("fecha_inicio")]
        public DateOnly FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateOnly FechaFin { get; set; }

        [Required]
        [StringLength(15)]
        [Column("estado")]
        public string Estado { get; set; } = EstadoMembresia.PendientePago.ToString();

        [Column("fecha_compra")]
        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

        [Column("auto_renovar")]
        public bool AutoRenovar { get; set; } = false;

        // Navegación
        [ForeignKey("IdTipoMembresia")]
        public virtual TipoMembresia? TipoMembresia { get; set; }
        
    }
}