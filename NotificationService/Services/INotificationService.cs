// Services/INotificationService.cs
using NotificationService.Dtos;
using NotificationService.Models; // Or DTOs if service works with DTOs

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto notificationDto);
    Task<NotificationDto?> GetNotificationByIdAsync(Guid id);
    Task<IEnumerable<NotificationDto>> GetNotificationsByUserIdAsync(int userId);
}