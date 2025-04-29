using BookingManagementService.Entities;

namespace BookingManagementService.Services;

public interface INotificationService
{
    Task SendMissedReservationNotificationAsync(Usuario user, ReservaMaquina reservation);
}