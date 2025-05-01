using BookingManagementService.Data;
using BookingManagementService.Models;
using BookingManagementService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingManagementService.HostedServices;

public class MissedReservationChecker : BackgroundService
{
    private readonly ILogger<MissedReservationChecker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes
    private const int GracePeriodMinutes = 15;

    public MissedReservationChecker(ILogger<MissedReservationChecker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Missed Reservation Checker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForMissedReservationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the Missed Reservation Checker background task.");
            }

            // Wait for the next check interval
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Missed Reservation Checker stopped.");
    }

    private async Task CheckForMissedReservationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking for missed machine reservations...");

        // Create a scope to resolve scoped services like DbContext and NotificationService
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            // Resolve IBookingService if you prefer calling its Update method
            // var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();


            var gracePeriodTime = DateTime.UtcNow.AddMinutes(-GracePeriodMinutes);

            var statusesToCheck = new[] { "Confirmada", "Pendiente" };

            var missedReservations = await dbContext.ReservasMaquinas
                .Include(r => r.Usuario) // Need user info for notification
                .Include(r => r.MaquinaEjercicio) // Need machine info for notification description
                .Where(r => r.FechaHoraInicio <= gracePeriodTime &&
                            statusesToCheck.Contains(r.Estado) && 
                            r.Asistio == null)
                .ToListAsync(cancellationToken);

            if (!missedReservations.Any())
            {
                _logger.LogInformation("No missed reservations found in this check.");
                return;
            }

             _logger.LogInformation("Found {Count} potentially missed reservations.", missedReservations.Count);


            foreach (var reservation in missedReservations)
            {
                 if (cancellationToken.IsCancellationRequested) break;

                _logger.LogWarning("Reservation {ReservationId} for user {UserId} potentially missed. Marking as Cancelada.", reservation.IdReservaMaquina, reservation.IdUsuario);

                reservation.Estado = "Cancelada";
                reservation.Asistio = false;

                if (reservation.Usuario != null && reservation.MaquinaEjercicio != null) // Ensure data loaded
                {
                    var notification = new NotificationRequest
                    {
                        IdUsuario = reservation.IdUsuario,
                        Tipo = "ReservacionPerdida",
                        Nombre = "Reservación Perdida",
                        Descripcion = $"No asististe a tu reservación de la máquina '{reservation.MaquinaEjercicio.Nombre}' programada para {reservation.FechaHoraInicio.ToLocalTime():g}. La reservación ha sido marcada como no asistida."
                    };
                    // Use the injected service instance (processor or directly in checker)
                    await notificationService.SendNotificationAsync(notification);
                }
                else {
                    _logger.LogError("Cannot send notification for reservation {ReservationId} because User or Machine data is missing.", reservation.IdReservaMaquina);
                }
            }

            try
            {
                 await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully processed {Count} missed reservations.", missedReservations.Count);
            }
            catch (DbUpdateException ex)
            {
                 _logger.LogError(ex, "Failed to save changes after processing missed reservations.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cancellation requested during saving missed reservation changes.");
            }
        }
    }
}