using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementService.Models
{
    [Table("Gimnasios")] // Maps to the specific table name
    public class Gimnasio
    {
        [Key] // Primary Key
        [Column("id_gimnasio")] // Maps to the specific column name
        public int IdGimnasio { get; set; }

        [Required] // Corresponds to NOT NULL
        [StringLength(150)]
        public string Nombre { get; set; } = null!; // Initialize non-nullable strings

        [Required]
        public string Direccion { get; set; } = null!;

        [Column("capacidad_maxima")]
        public int CapacidadMaxima { get; set; } = 100; // Matches default

        public bool Activo { get; set; } = true; // Matches default

        // Navigation Properties (relationships)
        public virtual ICollection<HorarioGimnasio>? Horarios { get; set; }
        public virtual ICollection<GimnasioAdministrador>? Administradores { get; set; }
        public virtual ICollection<EntrenadorGimnasio>? Entrenadores { get; set; }
        // Add others if managed here, e.g., EspaciosDeportivos
    }
}