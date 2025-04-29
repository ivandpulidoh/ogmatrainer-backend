using AuthenticationService.Data;
using AuthenticationService.Dtos;
using AuthenticationService.Models;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthController> _logger; // Optional: For logging

        public AuthController(
            AuthDbContext context,
            PasswordHasher passwordHasher,
            TokenService tokenService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == request.Email);
            if (existingUser != null)
            {
                return Conflict(new AuthResponseDto { IsSuccess = false, Message = "Email already exists." });
            }

            // Hash the password
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var newUser = new Usuarios
            {
                email = request.Email,
                password_hash = passwordHash
                // Map other fields from request if necessary
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

             _logger.LogInformation($"User registered successfully: {newUser.email}");

            
            var token = _tokenService.GenerateJwtToken(newUser);
            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Registration successful.", Token = token, UserId = newUser.id_usuario, Email = newUser.email });
           
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == request.Email);

            if (user == null)
            {
                 _logger.LogWarning($"Login failed for email: {request.Email}. User not found.");
                 return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials." });
            }

            // Verify the password
            var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.password_hash);

            if (!isPasswordValid)
            {
                _logger.LogWarning($"Login failed for email: {request.Email}. Invalid password.");
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials." });
            }

            // Generate JWT Token
            var token = _tokenService.GenerateJwtToken(user);

            _logger.LogInformation($"User logged in successfully: {user.email}");

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful.",
                Token = token,
                UserId = user.id_usuario, // Include User ID in response
                Email = user.email // Include Email in response
            });
        }
    }
}