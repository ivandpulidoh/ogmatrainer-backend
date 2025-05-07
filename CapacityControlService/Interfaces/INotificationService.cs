using CapacityControlService.Dtos;

namespace CapacityControlService.Interfaces;
public interface INotificationService {
     Task SendNotificationAsync(NotificationRequest notification);
}