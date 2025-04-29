using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    public class UserCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string? Nombre { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Apellido { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)] // Example password policy
        public string Password { get; set; } = null!;

        [Required]
        public List<string> Roles { get; set; } = new List<string>(); // e.g., ["Cliente"] or ["Entrenador", "AdminGimnasio"]

        // Add other fields required on creation if necessary
        public DateOnly? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Telefono { get; set; }
    }
}