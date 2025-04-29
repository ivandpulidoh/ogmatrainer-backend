using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagementService.Dtos;
using UserManagementService.Services;

namespace UserManagementService.Controllers
{
    [Route("api/users/{userId:int}/personal-information")] // Nested route
    [ApiController]
    [Authorize] // Require authentication for all actions in this controller
    public class PersonalInformationController : ControllerBase
    {
        private readonly IPersonalInformationService _piService;
        private readonly ILogger<PersonalInformationController> _logger;

        // Constants for Roles
        private const string AdminRole = "Administrador";
        private const string GymAdminRole = "AdminGimnasio";        
        private const string TrainerRole = "Entrenador";

        public PersonalInformationController(IPersonalInformationService piService, ILogger<PersonalInformationController> logger)
        {
            _piService = piService ?? throw new ArgumentNullException(nameof(piService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the personal fitness and health information for a specific user.
        /// Allowed for the user themselves or Admins.
        /// </summary>
        /// <param name="userId">The ID of the user whose information is requested.</param>
        /// <returns>The user's personal information.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PersonalInformationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PersonalInformationDto>> GetPersonalInformation(int userId)
        {
            if (!IsSelfOrAdmin(userId))
            {
                _logger.LogWarning("Forbidden attempt to access personal info for user {RequestedUserId} by user {LoggedInUserId}", userId, GetCurrentUserId());
                return Forbid();
            }

            _logger.LogInformation("Request received for GetPersonalInformation for User ID: {UserId}", userId);
            var info = await _piService.GetPersonalInformationAsync(userId);

            if (info == null)
            {
                 _logger.LogWarning("Personal information not found for User ID: {UserId}", userId);
                return NotFound("No se encontró información personal para el ID de usuario: "+userId);
            }

            return Ok(info);
        }

        /// <summary>
        /// Creates or updates the personal fitness and health information for a specific user.
        /// Allowed for the user themselves or Admins.
        /// </summary>
        /// <param name="userId">The ID of the user whose information is being set.</param>
        /// <param name="piDto">The personal information data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // If user doesn't exist
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpsertPersonalInformation(int userId, [FromBody] PersonalInformationDto piDto)
        {
             if (!ModelState.IsValid)
            {
                 _logger.LogWarning("UpsertPersonalInformation failed for User ID {UserId} due to invalid model state.", userId);
                return BadRequest(ModelState);
            }

            if (!IsSelfOrAdmin(userId))
            {
                _logger.LogWarning("Forbidden attempt to update personal info for user {RequestedUserId} by user {LoggedInUserId}", userId, GetCurrentUserId());
                return Forbid();
            }

            _logger.LogInformation("Attempting to upsert personal information for User ID: {UserId}", userId);
            var success = await _piService.UpsertPersonalInformationAsync(userId, piDto);

            if (!success)
            {
                // Could be NotFound (user doesn't exist) or other DB error
                _logger.LogWarning("UpsertPersonalInformation failed for User ID {UserId}. User may not exist or database error occurred.", userId);
                return NotFound(new { message = "Usuario no encontrado o error al guardar la información personal." });
            }

            return NoContent(); // Success
        }

        // --- Helper Methods for Authorization ---

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
             _logger.LogWarning("Could not parse User ID from claims in PersonalInformationController.");
            return null;
        }

        private bool IsAdmin()
        {
            // Check if user has either Admin or GymAdmin role
            return User.IsInRole(AdminRole) || User.IsInRole(GymAdminRole);
        }

        private bool IsSelfOrAdmin(int requestedUserId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return false;
            }

            // Allow if the logged-in user is the requested user OR if the logged-in user is an admin
            return currentUserId.Value == requestedUserId || IsAdmin();                       
           
        }
    }
}