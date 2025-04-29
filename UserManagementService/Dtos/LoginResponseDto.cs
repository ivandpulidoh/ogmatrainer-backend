using System.Collections.Generic;

namespace UserManagementService.Dtos
{
    /// <summary>
    /// DTO returned upon successful user authentication.
    /// Contains the JWT token and basic user identifying information.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// The generated JSON Web Token (JWT) for authenticated requests.
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// The unique identifier of the logged-in user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The email address of the logged-in user.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// The first name of the logged-in user (optional, but often useful).
        /// </summary>
        public string? Nombre { get; set; }

        /// <summary>
        /// The last name of the logged-in user (optional, but often useful).
        /// </summary>
        public string? Apellido { get; set; }

        /// <summary>
        /// List of roles assigned to the logged-in user.
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }
}