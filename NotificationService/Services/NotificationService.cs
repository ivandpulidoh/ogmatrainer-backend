using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Dtos;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationDbContext _context;
        
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(NotificationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto notificationDto)
        {
            
            var notification = new Notificacion
            {
                Id = Guid.NewGuid(),
                IdUsuario = notificationDto.IdUsuario,
                Tipo = notificationDto.Tipo,
                Nombre = notificationDto.Nombre,
                Descripcion = notificationDto.Descripcion,
                
                Fecha = DateTime.UtcNow
            };

            try
            {
                _context.Notificaciones.Add(notification);
                await _context.SaveChangesAsync();

                return MapToDto(notification);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to save notification to the database.", ex);
            }
        }

         public async Task<IEnumerable<NotificationDto>> GetNotificationsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching notifications for User ID: {UserId}", userId);

                var notifications = await _context.Notificaciones
                    .AsNoTracking() // Improves performance for read-only queries
                    .Where(n => n.IdUsuario == userId)
                    .OrderByDescending(n => n.Fecha)
                    .Select(n => MapToDto(n))
                    .ToListAsync();

                _logger.LogInformation("Found {Count} notifications for User ID: {UserId}", notifications.Count, userId);
                return notifications;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error fetching notifications for User ID: {UserId}", userId);
                 throw;
            }
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching notification by ID: {NotificationId}", id);

                // FindAsync is efficient for primary key lookups
                var notification = await _context.Notificaciones.FindAsync(id);

                if (notification == null)
                {
                    _logger.LogWarning("Notification not found for ID: {NotificationId}", id);
                    return null; // Indicate not found
                }

                _logger.LogInformation("Notification found for ID: {NotificationId}", id);
                // Map the found entity to a DTO
                return MapToDto(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notification by ID: {NotificationId}", id);
                throw; // Re-throw to be handled by the controller
            }
        }

        // Helper method to map Entity to DTO
        private static NotificationDto MapToDto(Notificacion n)
        {
            return new NotificationDto(
                n.Id,
                n.IdUsuario,
                n.Tipo,
                n.Fecha,
                n.Nombre,
                n.Descripcion
            );
        }
    }
}