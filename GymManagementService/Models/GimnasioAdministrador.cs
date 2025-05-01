using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementService.Models
{
    [Table("GimnasioAdministradores")]
    // No primary key attribute here - will be defined in DbContext
    public class GimnasioAdministrador
    {
        [Required]
        [Column("id_gimnasio")]
        public int IdGimnasio { get; set; }

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; } // Represents the Admin user's ID

        // Navigation properties
        [ForeignKey("IdGimnasio")]
        public virtual Gimnasio? Gimnasio { get; set; }

        // No direct navigation to Usuario here unless you define a basic Usuario model
        // Often, microservices only store the ID of users from another service.
    }
}