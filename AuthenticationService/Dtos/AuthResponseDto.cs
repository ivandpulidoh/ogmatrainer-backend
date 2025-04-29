// Dtos/AuthResponseDto.cs
namespace AuthenticationService.Dtos
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; } // The JWT
        public int? UserId { get; set; } // Optionally return user ID
        public string? Email { get; set; } // Optionally return email
    }
}