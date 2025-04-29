using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    /// <summary>
    /// DTO specifically for updating a user's password.
    /// </summary>
    public class UserPasswordUpdateDto
    {
        [Required(ErrorMessage = "La nueva contraseña es requerida.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        // Add more complexity requirements via Regular Expressions if needed
        // [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        //    ErrorMessage = "La contraseña debe tener mayúsculas, minúsculas, números y símbolos.")]
        public string NewPassword { get; set; } = null!;
    }
}