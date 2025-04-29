using BookingManagementService.Data;
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
    private readonly IServiceProvider _serviceProvider; // To create scopes for DbContext/Services
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

            // Find reservations that started more than 15 mins ago, are still 'Confirmada',
            // and haven't been marked as attended/unattended yet (Asistio is NULL).
            var missedReservations = await dbContext.ReservasMaquinas
                .Include(r => r.Usuario) // Need user info for notification
                .Include(r => r.MaquinaEjercicio) // Need machine info for notification description
                .Where(r => r.FechaHoraInicio <= gracePeriodTime &&
                            r.Estado == "Confirmada" &&
                            r.Asistio == null)
                .ToListAsync(cancellationToken); // Pass cancellation token

            if (!missedReservations.Any())
            {
                _logger.LogInformation("No missed reservations found in this check.");
                return;
            }

             _logger.LogInformation("Found {Count} potentially missed reservations.", missedReservations.Count);


            foreach (var reservation in missedReservations)
            {
                 if (cancellationToken.IsCancellationRequested) break; // Check before processing each

                _logger.LogWarning("Reservation {ReservationId} for user {UserId} potentially missed. Marking as NoShow.", reservation.IdReservaMaquina, reservation.IdUsuario);

                // Update status directly or call service method
                reservation.Estado = "NoShow";
                reservation.Asistio = false; // Mark explicitly as not attended

                // Send notification
                if (reservation.Usuario != null) // Ensure user was loaded
                {
                    await notificationService.SendMissedReservationNotificationAsync(reservation.Usuario, reservation);
                }
                else
                {
                    _logger.LogError("Cannot send notification for reservation {ReservationId} because User data is missing.", reservation.IdReservaMaquina);
                }

                 // Optional: Add error handling around notification sending if needed
            }

            try
            {
                // Save all changes made in this scope
                 await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully processed {Count} missed reservations.", missedReservations.Count);
            }
            catch (DbUpdateException ex)
            {
                 _logger.LogError(ex, "Failed to save changes after processing missed reservations.");
                 // Consider retry logic or specific error handling here
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cancellation requested during saving missed reservation changes.");
            }
        } // Scope is disposed here, releasing DbContext etc.
    }
}