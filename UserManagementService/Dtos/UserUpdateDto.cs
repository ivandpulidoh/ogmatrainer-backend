using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    /// <summary>
    /// DTO for updating an existing user's profile information.
    /// Password changes should be handled via a separate dedicated endpoint/DTO.
    /// Role updates are handled here by providing the complete list of desired roles.
    /// </summary>
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        [MaxLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        [MaxLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres.")]
        public string Email { get; set; } = null!;

        // Optional fields below
        public DateOnly? FechaNacimiento { get; set; }

        [MaxLength(20, ErrorMessage = "El género no puede exceder los 20 caracteres.")]
        public string? Genero { get; set; }

        [MaxLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
        public string? Direccion { get; set; }

        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")] // Basic phone format validation
        public string? Telefono { get; set; }

        [MaxLength(255, ErrorMessage = "La URL de la foto no puede exceder los 255 caracteres.")]
        [Url(ErrorMessage = "El formato de la URL de la foto no es válido.")] // Basic URL validation
        public string? FotoUrl { get; set; }

        /// <summary>
        /// The complete list of roles the user should have after the update.
        /// Roles not included in this list will be removed.
        /// Roles included that the user doesn't have will be added.
        /// Provide an empty list to remove all roles (consider if this is allowed).
        /// </summary>
        [Required(ErrorMessage = "La lista de roles es requerida (puede estar vacía).")]
        public List<string> Roles { get; set; } = new List<string>();

        // Note: 'Activo' is typically managed via specific activation/deactivation endpoints,
        // and Password via a password change endpoint, so they are omitted here.
    }
}