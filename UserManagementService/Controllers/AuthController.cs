using Microsoft.AspNetCore.Mvc;
using UserManagementService.Dtos;
using UserManagementService.Services; // Use the service interface

namespace UserManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginResponse = await _authService.LoginAsync(loginDto);

            if (loginResponse == null)
            {
                 _logger.LogWarning("Login failed for email: {Email}", loginDto.Email);
                return Unauthorized(new { message = "Invalid credentials or user inactive." });
            }

             _logger.LogInformation("Successful login for email: {Email}", loginDto.Email);
            return Ok(loginResponse);
        }

        // Optional: Add a registration endpoint here that calls the UserService.CreateUserAsync
        // [HttpPost("register")]
        // public async Task<IActionResult> Register([FromBody] UserCreateDto registrationDto) { ... }
    }
}