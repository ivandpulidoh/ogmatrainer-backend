using Microsoft.AspNetCore.Mvc;
using NotificationService.Dtos; // Assuming DTOs are in this namespace
using NotificationService.Services; // Assuming service interface is here

namespace NotificationService.Controllers
{
    [ApiController] // Enables API-specific behaviors like automatic model validation
    [Route("api/[controller]")] // Defines the base route: /api/notifications
    public class NotificationsController : ControllerBase // Inherit from ControllerBase for API controllers
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger; // Optional: for logging

        // Inject dependencies via the constructor
        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="notificationDto">Data for the notification to create.</param>
        /// <returns>The created notification resource.</returns>
        [HttpPost] // Maps to HTTP POST requests on /api/notifications
        [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)] // Swagger docs: Success
        [ProducesResponseType(StatusCodes.Status400BadRequest)]                     // Swagger docs: Validation error
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]            // Swagger docs: Server error
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto notificationDto)
        {
            // ModelState validation is often handled automatically due to [ApiController]
            // but you can add custom checks if needed.
            if (!ModelState.IsValid)
            {
                 return BadRequest(ModelState);
            }

            try
            {
                var createdNotification = await _notificationService.CreateNotificationAsync(notificationDto);

                _logger.LogInformation("Notification created successfully with ID: {NotificationId}", createdNotification.Id);

                // Return 201 Created status with a Location header pointing to where the
                // new resource can be retrieved (assuming a GetById endpoint exists or will exist)
                // and include the created resource in the body.
                return CreatedAtAction(nameof(GetNotificationById), // Name of the action to get the resource
                                       new { id = createdNotification.Id }, // Route values for the Get action
                                       createdNotification); // The created resource DTO
            }
            catch (ArgumentException ex) // Example: Catch specific validation errors from the service
            {
                 _logger.LogWarning("Bad request creating notification: {ErrorMessage}", ex.Message);
                 // Return 400 Bad Request with the error message
                 return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) // Catch unexpected errors
            {
                _logger.LogError(ex, "An error occurred while creating notification for user {UserId}", notificationDto.IdUsuario);
                // Return 500 Internal Server Error status code with a generic error message
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Gets a specific notification by its ID. (Placeholder for CreatedAtAction)
        /// </summary>
        /// <param name="id">The Guid ID of the notification.</param>
        /// <returns>The notification if found; otherwise NotFound.</returns>
        [HttpGet("{id:guid}", Name = "GetNotificationById")] // Route: /api/notifications/{guid}
        [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNotificationById(Guid id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null) return NotFound();
            return Ok(notification);
        }

         // --- New GET GetNotificationsByUser Action ---
        /// <summary>
        /// Gets all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of notifications for the user.</returns>
        [HttpGet("user/{userId:int}")] // Route: /api/notifications/user/{userId}
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // If userId is invalid (e.g., <= 0)
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNotificationsByUser(int userId)
        {
            _logger.LogInformation("Attempting to retrieve notifications for User ID: {UserId}", userId);

            // Optional: Basic validation for the user ID
            if (userId <= 0)
            {
                 _logger.LogWarning("Invalid User ID requested: {UserId}", userId);
                 return BadRequest(new { message = "Invalid user ID provided." });
            }

            try
            {
                var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);

                // It's standard to return an empty list with 200 OK if no records are found for a collection query.
                // No need to return NotFound unless the user *itself* doesn't exist,
                // which would require checking the Users table (outside this service's scope).
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                 // The service layer already logged the details
                 _logger.LogError(ex, "An error occurred while retrieving notifications for User ID: {UserId}", userId);
                 return StatusCode(StatusCodes.Status500InternalServerError,
                                   new { message = "An error occurred while retrieving notifications." });
            }
        }

    }
}