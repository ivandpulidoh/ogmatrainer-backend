using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementService.Models
{
    [Table("Gimnasios")]
    public class Gimnasio
    {
        [Key]
        [Column("id_gimnasio")]
        public int IdGimnasio { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Direccion { get; set; } = null!;

        [Column("capacidad_maxima")]
        public int CapacidadMaxima { get; set; } = 100;

        public bool Activo { get; set; } = true;

        [Column("codigo_qr_entrada")]
        public byte[]? CodigoQrEntrada { get; set; }

        [Column("codigo_qr_salida")]
        public byte[]? CodigoQrSalida { get; set; }

        [Column("fomulario_obligatorio")]
        public bool FormularioObligatorio { get; set; }

        public virtual ICollection<HorarioGimnasio>? Horarios { get; set; }
        public virtual ICollection<GimnasioAdministrador>? Administradores { get; set; }
        public virtual ICollection<EntrenadorGimnasio>? Entrenadores { get; set; }
        
    }
}