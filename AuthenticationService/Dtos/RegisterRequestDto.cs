// Dtos/RegisterRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Dtos
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)] // Add password complexity rules as needed
        public string Password { get; set; } = string.Empty;

         // Add other fields needed during registration if any (Name, etc.)
         // public string? Name { get; set; }
    }
}
