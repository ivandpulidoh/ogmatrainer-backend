using UserManagementService.Dtos;
using System.Threading.Tasks; // Added for Task

namespace UserManagementService.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Attempts to authenticate a user based on email and password.
        /// </summary>
        /// <param name="loginDto">The user's login credentials.</param>
        /// <returns>A DTO containing the JWT and basic user info if successful, otherwise null.</returns>
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto);

        // Could add methods for:
        // - Refresh Token generation/validation
        // - Password Reset flow initiation/completion
        // - Email Verification token generation/validation
    }
}