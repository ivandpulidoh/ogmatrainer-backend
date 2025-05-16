using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementService.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nombre")]
        [MaxLength(100)]
        public string? Nombre { get; set; } // Nullable if allowed

        [Column("apellido")]
        [MaxLength(100)]
        public string? Apellido { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("password_hash")]
        [MaxLength(255)]
        public string? PasswordHash { get; set; } // Store HASHED passwords

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("foto_url")]
        [MaxLength(255)]
        public string? FotoUrl { get; set; }

        [Column("fecha_nacimiento")]
        public DateOnly? FechaNacimiento { get; set; } // Use DateOnly for Date type

        [Column("genero")]
        [MaxLength(20)]
        public string? Genero { get; set; }

        [Column("direccion")]
        [MaxLength(255)]
        public string? Direccion { get; set; }

        [Column("telefono")]
        [MaxLength(20)]
        public string? Telefono { get; set; }

        // ... (OAuth, Email Verification, Penalty fields as needed) ...

        [Column("alertas_no_asistencia")]
        public int AlertasNoAsistencia { get; set; } = 0;

        [Column("penalizado_hasta")]
        public DateTime? PenalizadoHasta { get; set; }        
    }
}