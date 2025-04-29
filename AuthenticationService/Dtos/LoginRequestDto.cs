// Dtos/LoginRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Dtos
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}