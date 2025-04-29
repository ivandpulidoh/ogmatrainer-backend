using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Added for IConfiguration
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens; // Added for SecurityKey etc.
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // Added for JwtSecurityTokenHandler
using System.Linq;
using System.Security.Claims; // Added for Claims
using System.Text; // Added for Encoding
using System.Threading.Tasks;
using UserManagementService.Data;
using UserManagementService.Dtos;
using UserManagementService.Models;

namespace UserManagementService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManagementDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManagementDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            // 1. Find user by email (include roles!)
            var user = await _context.Usuarios
                                     .Include(u => u.UsuarioRoles)
                                        .ThenInclude(ur => ur.Rol)
                                     .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower()); // Case-insensitive comparison

            // 2. Validate user existence and state
            if (user == null) {
                _logger.LogWarning("Login failed: User not found for email {Email}", loginDto.Email);
                return null; // User not found
            }

            if (!user.Activo) {
                 _logger.LogWarning("Login failed: User account inactive for email {Email}", loginDto.Email);
                 return null; // User inactive
            }

            if (string.IsNullOrEmpty(user.PasswordHash)) {
                 _logger.LogWarning("Login failed: User {Email} has no password set.", loginDto.Email);
                 return null; // Cannot login without a password hash
            }


            // 3. Verify password
            bool isPasswordValid;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            }
            catch (Exception ex) // Catch potential BCrypt exceptions
            {
                _logger.LogError(ex, "Error during password verification for user {Email}", loginDto.Email);
                return null; // Treat verification error as login failure
            }


            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for email {Email}", loginDto.Email);
                // Consider adding rate limiting or lockout logic here for security
                return null; // Invalid password
            }

            // 4. Generate JWT Token upon successful validation
             _logger.LogInformation("Password verified successfully for email {Email}. Generating token.", loginDto.Email);
            var token = GenerateJwtToken(user);

            // 5. Return response DTO
            return new LoginResponseDto
            {
                Token = token,
                UserId = user.IdUsuario,
                Email = user.Email,
                Nombre = user.Nombre, // Add name if useful in response
                Apellido = user.Apellido,
                Roles = user.UsuarioRoles.Select(ur => ur.Rol.NombreRol).ToList()
            };
        }


        // --- Private Helper Method ---

        private string GenerateJwtToken(Usuario user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

             // Validate configuration presence
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                _logger.LogCritical("JWT Key, Issuer, or Audience is not configured correctly in appsettings.");
                throw new InvalidOperationException("JWT settings are missing or invalid.");
            }
            // Ensure key length is sufficient for HS256 (usually needs to be > 16 bytes, often 32 or 64 bytes recommended)
             if (Encoding.UTF8.GetBytes(key).Length < 16) // Simplistic check, adjust as needed
             {
                  _logger.LogCritical("JWT Key is too short. HS256 requires a longer key.");
                  throw new InvalidOperationException("JWT key configuration is insecure.");
             }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.IdUsuario.ToString()), // Subject == User ID
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()), // Common practice to include NameIdentifier
                // Optional: Add Name claim if needed often
                // new Claim(JwtRegisteredClaimNames.GivenName, user.Nombre ?? ""),
                // new Claim(JwtRegisteredClaimNames.FamilyName, user.Apellido ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique Token ID
            };

            // Add roles as claims
            foreach (var userRole in user.UsuarioRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Rol.NombreRol));
            }

            // Define Token expiration (make configurable)
            var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60); // Default 60 mins
            var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}