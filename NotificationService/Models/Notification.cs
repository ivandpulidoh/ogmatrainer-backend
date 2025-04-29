using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationService.Models
{
    public class Notificacion
    {
        [Key] // Marks Id as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // We will generate the Guid ourselves
        public Guid Id { get; set; } // UUID mapped to Guid in C#

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; } // Foreign key to Usuarios table

        [Required]
        [MaxLength(100)]
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty; // Type of notification

        [Required]
        [Column("fecha")]
        public DateTime Fecha { get; set; } // Date and time of creation

        [Required]
        [MaxLength(255)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
        [Column("descripcion")]
        public string? Descripcion { get; set; }
    
    }
}