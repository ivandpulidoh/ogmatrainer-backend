using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementService.Models
{
    [Table("EntrenadorGimnasios")]
    public class EntrenadorGimnasio
    {
        [Required]
        [Column("id_usuario")] // Represents the Trainer user's ID
        public int IdUsuario { get; set; }

        [Required]
        [Column("id_gimnasio")]
        public int IdGimnasio { get; set; }

        // Navigation properties
        [ForeignKey("IdGimnasio")]
        public virtual Gimnasio? Gimnasio { get; set; }
    }
}