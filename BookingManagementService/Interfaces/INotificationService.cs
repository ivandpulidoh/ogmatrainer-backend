using BookingManagementService.Entities;
using BookingManagementService.Models;

namespace BookingManagementService.Services;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationRequest notification);
}