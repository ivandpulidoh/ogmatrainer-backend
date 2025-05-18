using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MembershipService.Models
{
    [Table("TiposMembresia")]
    public class TipoMembresia
    {
        [Key]
        [Column("id_tipo_membresia")]
        public int IdTipoMembresia { get; set; }

        [Column("id_gimnasio")]
        public int IdGimnasio { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Column(TypeName = "NVARCHAR(MAX)")]
        public string? Descripcion { get; set; }

        [Column("duracion_meses")]
        public int DuracionMeses { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;
       
        public virtual ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();
    }
}