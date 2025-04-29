using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagementService.Data;
using UserManagementService.Dtos;
using UserManagementService.Models;

namespace UserManagementService.Services
{
    public class UserService : IUserService
    {
        private readonly UserManagementDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManagementDbContext context, ILogger<UserService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserReadDto?> GetUserByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to retrieve user with ID: {UserId}", id);
            var user = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .AsNoTracking() // Read-only operation
                .FirstOrDefaultAsync(u => u.IdUsuario == id && u.Activo); // Only find active users

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found or is inactive.", id);
                return null;
            }

            // Map Entity to DTO
            return MapUsuarioToReadDto(user);
        }

        public async Task<IEnumerable<UserReadDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Attempting to retrieve all active users.");
            var users = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .Where(u => u.Activo) // Only active users
                .AsNoTracking()
                .ToListAsync();

            // Map List<Entity> to List<DTO>
            return users.Select(MapUsuarioToReadDto).ToList();
        }

        public async Task<UserReadDto?> CreateUserAsync(UserCreateDto userDto)
        {
            _logger.LogInformation("Attempting to create user with email: {Email}", userDto.Email);

            // 1. Check if email already exists (case-insensitive check recommended)
            if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == userDto.Email.ToLower()))
            {
                _logger.LogWarning("User creation failed: Email {Email} already exists.", userDto.Email);
                return null; // Indicate failure: email exists
            }

            // 2. Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // 3. Create the Usuario entity
            var newUser = new Usuario
            {
                Nombre = userDto.Nombre,
                Apellido = userDto.Apellido,
                Email = userDto.Email,
                PasswordHash = passwordHash,
                FechaNacimiento = userDto.FechaNacimiento,
                Genero = userDto.Genero,
                Telefono = userDto.Telefono,                
                Activo = true,
                FechaRegistro = DateTime.UtcNow                
            };

            // 4. Handle Roles
            if (userDto.Roles != null && userDto.Roles.Any())
            {
                foreach (var roleName in userDto.Roles.Distinct()) // Use Distinct
                {
                    // Find role by name (case-insensitive recommended)
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol.ToLower() == roleName.ToLower());
                    if (role != null)
                    {
                        newUser.UsuarioRoles.Add(new UsuarioRol { IdRol = role.IdRol });
                    }
                    else
                    {
                        _logger.LogWarning("Role '{RoleName}' not found during user creation for {Email}", roleName, userDto.Email);
                        // Depending on requirements: could throw error, ignore, or create role
                    }
                }
            }
            else
            {
                 // Assign default role (e.g., 'Cliente') if none provided
                 var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Cliente");
                 if (defaultRole != null)
                 {
                     newUser.UsuarioRoles.Add(new UsuarioRol { IdRol = defaultRole.IdRol });
                 }
                 else
                 {
                     _logger.LogError("Default role 'Cliente' not found in database. Cannot assign default role.");
                     // Handle this critical configuration error
                 }
            }


            _context.Usuarios.Add(newUser);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User created successfully with ID: {UserId}", newUser.IdUsuario);

                // Fetch the user again with includes to return the complete DTO
                var createdUser = await _context.Usuarios
                                    .Include(u => u.UsuarioRoles)
                                    .ThenInclude(ur => ur.Rol)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.IdUsuario == newUser.IdUsuario);

                return createdUser != null ? MapUsuarioToReadDto(createdUser) : null; // Map to Read DTO
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving new user to database for email: {Email}", userDto.Email);
                return null; // Indicate failure
            }
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateDto userDto)
        {
            _logger.LogInformation("Attempting to update user with ID: {UserId}", id);
            var user = await _context.Usuarios
                .Include(u => u.UsuarioRoles) // Include roles for modification
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (user == null || !user.Activo)
            {
                _logger.LogWarning("Update failed: User with ID {UserId} not found or is inactive.", id);
                return false; // User not found or inactive
            }

            // Update basic properties (ensure email uniqueness if changed)
            if (!string.Equals(user.Email, userDto.Email, StringComparison.OrdinalIgnoreCase))
            {
                 // Check if the new email is already taken by another user
                 if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == userDto.Email.ToLower() && u.IdUsuario != id))
                 {
                     _logger.LogWarning("Update failed for User ID {UserId}: New email {Email} is already in use.", id, userDto.Email);
                     return false; // Email conflict
                 }
                 user.Email = userDto.Email;                 
            }

            user.Nombre = userDto.Nombre;
            user.Apellido = userDto.Apellido;
            user.FechaNacimiento = userDto.FechaNacimiento;
            user.Genero = userDto.Genero;
            user.Direccion = userDto.Direccion;
            user.Telefono = userDto.Telefono;
            user.FotoUrl = userDto.FotoUrl;
            // user.Activo = userDto.Activo; // Only allow changing 'Activo' via DeleteUserAsync or specific activation logic

            // Update Roles (Example: Replace existing roles with new ones)
             if (userDto.Roles != null)
             {
                 // Remove roles not in the new list
                 var rolesToRemove = user.UsuarioRoles
                                         .Where(ur => !userDto.Roles.Contains(ur.Rol.NombreRol, StringComparer.OrdinalIgnoreCase))
                                         .ToList();
                 _context.UsuarioRoles.RemoveRange(rolesToRemove);

                 // Add new roles
                 var currentRoleNames = user.UsuarioRoles.Select(ur => ur.Rol.NombreRol.ToLower()).ToList();
                 foreach (var roleName in userDto.Roles.Distinct())
                 {
                     if (!currentRoleNames.Contains(roleName.ToLower()))
                     {
                         var role = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol.ToLower() == roleName.ToLower());
                         if (role != null)
                         {
                              user.UsuarioRoles.Add(new UsuarioRol { IdRol = role.IdRol });
                         }
                         else
                         {
                              _logger.LogWarning("Role '{RoleName}' not found during user update for {UserId}", roleName, id);
                         }
                     }
                 }
             }
             // Else: If userDto.Roles is null, maybe don't change roles? Define behavior.


            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID: {UserId} updated successfully.", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                 _logger.LogError(ex, "Concurrency conflict updating user with ID: {UserId}.", id);
                return false; // Concurrency issue
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating user with ID: {UserId}.", id);
                return false; // Other DB error
            }
        }

         public async Task<bool> UpdateUserPasswordAsync(int id, string newPassword)
         {
             _logger.LogInformation("Attempting to update password for user ID: {UserId}", id);
             var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);

             if (user == null || !user.Activo)
             {
                 _logger.LogWarning("Password update failed: User with ID {UserId} not found or is inactive.", id);
                 return false;
             }

             if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8) // Basic check
             {
                 _logger.LogWarning("Password update failed for User ID {UserId}: Invalid password provided.", id);
                 return false; // Add more robust validation as needed
             }

             user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

             try
             {
                 await _context.SaveChangesAsync();
                 _logger.LogInformation("Password updated successfully for user ID: {UserId}", id);
                 return true;
             }
             catch (DbUpdateException ex)
             {
                 _logger.LogError(ex, "Database error updating password for user ID: {UserId}", id);
                 return false;
             }
         }

        public async Task<bool> DeleteUserAsync(int id)
        {
            _logger.LogInformation("Attempting to soft delete user with ID: {UserId}", id);
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (user == null)
            {
                _logger.LogWarning("Soft delete failed: User with ID {UserId} not found.", id);
                return false; // Not found
            }

            if (!user.Activo)
            {
                _logger.LogInformation("User with ID {UserId} is already inactive.", id);
                return true; // Already inactive, consider it a success in terms of state
            }

            user.Activo = false; // Soft delete

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID: {UserId} marked as inactive.", id);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during soft delete for user ID: {UserId}", id);
                return false;
            }
        }

        // --- Private Helper Methods ---

        private UserReadDto MapUsuarioToReadDto(Usuario user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return new UserReadDto
            {
                IdUsuario = user.IdUsuario,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                Activo = user.Activo,
                FechaRegistro = user.FechaRegistro,
                FotoUrl = user.FotoUrl,
                FechaNacimiento = user.FechaNacimiento,
                Genero = user.Genero,
                Direccion = user.Direccion,
                Telefono = user.Telefono,
                // Map penalty info if needed in DTO
                AlertasNoAsistencia = user.AlertasNoAsistencia,
                PenalizadoHasta = user.PenalizadoHasta,
                Roles = user.UsuarioRoles?.Select(ur => ur.Rol?.NombreRol ?? "Unknown").ToList() ?? new List<string>()
                // Map other fields as required by UserReadDto
            };
        }
    }
}