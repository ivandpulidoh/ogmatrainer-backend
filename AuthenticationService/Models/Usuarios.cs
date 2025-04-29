using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models
{
    public class Usuarios
    {
        public int id_usuario { get; set; } // Corresponds to id_usuario

        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password_hash { get; set; } = string.Empty; // Store HASHED password

        // Add other relevant fields if needed directly for auth (e.g., IsActive)
        // public bool IsActive { get; set; } = true;
        // public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    }
}