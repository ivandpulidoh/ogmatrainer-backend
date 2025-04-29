using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Added for logging
using System;
using System.Collections.Generic; // Added for IEnumerable
using System.Security.Claims; // Added for User Claims
using System.Threading.Tasks; // Added for Task
using UserManagementService.Dtos;
using UserManagementService.Services;

namespace UserManagementService.Controllers
{
    [Route("api/[controller]")] // Route: /api/users
    [ApiController]
    [Authorize] // Require authentication for most actions by default
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        // Constants for Roles (avoids magic strings)
        private const string AdminRole = "Administrador";
        private const string GymAdminRole = "AdminGimnasio";

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a list of all active users. Requires Admin or GymAdmin role.
        /// </summary>
        /// <returns>A list of users.</returns>
        [HttpGet]
        [Authorize(Roles = $"{AdminRole},{GymAdminRole}")] // Only Admins can get all users
        [ProducesResponseType(typeof(IEnumerable<UserReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAllUsers()
        {
            _logger.LogInformation("Admin request received for GetAllUsers.");
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Gets a specific user by their ID. Allowed for the user themselves or Admins.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The user's details.</returns>
        [HttpGet("{id:int}", Name = "GetUserById")] // Named route for CreatedAtAction
        [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserReadDto>> GetUserById(int id)
        {
            if (!IsSelfOrAdmin(id))
            {
                _logger.LogWarning("Forbidden attempt to access user {RequestedUserId} by user {LoggedInUserId}.", id, GetCurrentUserId());
                return Forbid(); // User is authenticated but not authorized for this specific resource
            }

            _logger.LogInformation("Request received for GetUserById: {UserId}", id);
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("GetUserById: User with ID {UserId} not found.", id);
                return NotFound();
            }

            return Ok(user);
        }

        /// <summary>
        /// Creates a new user. Can be public or admin-only depending on the [Authorize]/[AllowAnonymous] attribute.
        /// </summary>
        /// <param name="userDto">The details of the user to create.</param>
        /// <returns>The newly created user's details.</returns>
        [HttpPost]
        [AllowAnonymous] // Example: Allow public registration. Change to [Authorize(Roles=AdminRole)] if needed.
        [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserReadDto>> CreateUser([FromBody] UserCreateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                 _logger.LogWarning("CreateUser failed due to invalid model state.");
                return BadRequest(ModelState);
            }

             _logger.LogInformation("Attempting to create user with email: {Email}", userDto.Email);
            var createdUser = await _userService.CreateUserAsync(userDto);

            if (createdUser == null)
            {
                // Could be duplicate email or other service-layer issue
                 _logger.LogWarning("User creation failed for email: {Email}. Potentially duplicate email.", userDto.Email);
                return BadRequest(new { message = "No se pudo crear el usuario. El email podría ya estar en uso o los roles no son válidos." });
            }

            // Return 201 Created with a location header pointing to the new resource
            return CreatedAtRoute("GetUserById", new { id = createdUser.IdUsuario }, createdUser);
        }

        /// <summary>
        /// Updates an existing user's profile information (excluding password). Allowed for the user themselves or Admins.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="userDto">The updated user data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateUser failed for ID {UserId} due to invalid model state.", id);
                return BadRequest(ModelState);
            }

            if (!IsSelfOrAdmin(id))
            {
                _logger.LogWarning("Forbidden attempt to update user {RequestedUserId} by user {LoggedInUserId}.", id, GetCurrentUserId());
                return Forbid();
            }

            _logger.LogInformation("Attempting to update user with ID: {UserId}", id);
            var success = await _userService.UpdateUserAsync(id, userDto);

            if (!success)
            {
                 _logger.LogWarning("UpdateUser failed for ID {UserId}. User not found or email conflict.", id);
                // Could be NotFound or BadRequest (e.g., email conflict)
                // Returning NotFound for simplicity, but could refine based on service feedback
                return NotFound(new { message = "Usuario no encontrado o no se pudo actualizar (ej: conflicto de email)." });
            }

            return NoContent(); // Standard success response for PUT update
        }


        /// <summary>
        /// Updates the password for a specific user. Allowed for the user themselves or Admins.
        /// </summary>
        /// <param name="id">The ID of the user whose password is to be updated.</param>
        /// <param name="passwordDto">Object containing the new password.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id:int}/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UserPasswordUpdateDto passwordDto)
        {
             if (!ModelState.IsValid)
            {
                 _logger.LogWarning("UpdatePassword failed for ID {UserId} due to invalid model state.", id);
                return BadRequest(ModelState);
            }

            if (!IsSelfOrAdmin(id))
            {
                _logger.LogWarning("Forbidden attempt to update password for user {RequestedUserId} by user {LoggedInUserId}.", id, GetCurrentUserId());
                return Forbid();
            }

             _logger.LogInformation("Attempting to update password for user ID: {UserId}", id);
             var success = await _userService.UpdateUserPasswordAsync(id, passwordDto.NewPassword);

             if (!success)
             {
                  _logger.LogWarning("UpdatePassword failed for ID {UserId}. User not found or invalid password.", id);
                 return NotFound(new { message = "Usuario no encontrado o la contraseña no es válida."}); // User not found or service rejected password
             }

            return NoContent();
        }


        /// <summary>
        /// Deactivates a user (soft delete). Requires Admin role.
        /// </summary>
        /// <param name="id">The ID of the user to deactivate.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = AdminRole)] // Only full Admins can delete users
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation("Admin request received to delete user with ID: {UserId}", id);
            var success = await _userService.DeleteUserAsync(id);

            if (!success)
            {
                _logger.LogWarning("DeleteUser failed: User with ID {UserId} not found.", id);
                return NotFound();
            }

            return NoContent(); // Standard success response for DELETE
        }

        // --- Helper Methods for Authorization ---

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"); // "sub" is often used too
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            _logger.LogWarning("Could not parse User ID from claims.");
            return null; // User ID not found in token or invalid format
        }

        private bool IsAdmin()
        {
            return User.IsInRole(AdminRole) || User.IsInRole(GymAdminRole);
        }

        private bool IsSelfOrAdmin(int requestedUserId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return false; // Cannot verify if user ID is missing from token
            }

            return currentUserId.Value == requestedUserId || IsAdmin();
        }
    }
}