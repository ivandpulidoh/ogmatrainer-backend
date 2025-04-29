using System;
using System.Collections.Generic;

namespace UserManagementService.Dtos
{
    /// <summary>
    /// DTO for returning user information to clients.
    /// Excludes sensitive information like password hash.
    /// </summary>
    public class UserReadDto
    {
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string Email { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public string? FotoUrl { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public int AlertasNoAsistencia { get; set; }
        public DateTime? PenalizadoHasta { get; set; }

        /// <summary>
        /// List of roles assigned to the user.
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        // Explicitly does NOT include PasswordHash
    }
}