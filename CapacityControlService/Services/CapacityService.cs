using CapacityControlService.Data;
using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapacityControlService.Services;

public class CapacityService : ICapacityService
{
    private readonly CapacityDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CapacityService> _logger;
    private readonly IAdminFinderService _adminFinderService;

    // Simple in-memory flag to prevent spamming notifications (improve with distributed cache if scaled)
    private static readonly Dictionary<int, DateTime> _lastNotificationSent = new();
    private static readonly TimeSpan _notificationCooldown = TimeSpan.FromMinutes(15);


    public CapacityService(CapacityDbContext context, INotificationService notificationService, IConfiguration configuration, ILogger<CapacityService> logger, IAdminFinderService adminFinderService)
    {
        _context = context;
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;
        _adminFinderService = adminFinderService;
    }

    public async Task CheckCapacityAndNotifyAsync(int gymId, int currentOccupancy)
    {
        var gym = await _context.Gimnasios.FindAsync(gymId);
        if (gym == null || gym.CapacidadMaxima <= 0) return; // No valid gym or capacity

        double thresholdPercentage = _configuration.GetValue<double>("CapacityThreshold:WarningPercentage", 0.9);
        int warningThreshold = (int)Math.Floor(gym.CapacidadMaxima * thresholdPercentage);

        if (currentOccupancy >= warningThreshold)
        {
            _logger.LogWarning("Gym {GymId} is nearing capacity. Occupancy: {CurrentOccupancy}/{MaxCapacity} (Threshold: {Threshold})",
                gymId, currentOccupancy, gym.CapacidadMaxima, warningThreshold);

            // Check cooldown
            lock (_lastNotificationSent) // Basic thread safety
            {
                 if (_lastNotificationSent.TryGetValue(gymId, out var lastSent) && (DateTime.UtcNow - lastSent) < _notificationCooldown)
                 {
                      _logger.LogInformation("Capacity notification for Gym {GymId} skipped due to cooldown.", gymId);
                      return; // Still within cooldown period
                 }
                _lastNotificationSent[gymId] = DateTime.UtcNow; // Update last sent time
            }


             var allAdminIds = await _adminFinderService.GetAdminIdsForGymAsync(gymId);

            if (!allAdminIds.Any())
            {
                _logger.LogWarning("No administrators found for Gym {GymId} to send capacity warning.", gymId);
                return;
            }
           
            foreach (var adminId in allAdminIds)
            {
                var notification = new NotificationRequest
                {
                    IdUsuario = adminId,
                    Tipo = "GymCapacityWarning",
                    Nombre = $"Alerta de Capacidad: {gym.Nombre}",
                    Descripcion = $"El gimnasio '{gym.Nombre}' está alcanzando su capacidad máxima. Ocupación actual: {currentOccupancy}/{gym.CapacidadMaxima}."
                };
                await _notificationService.SendNotificationAsync(notification);
            }
        }
         else
         {
             // Optional: If occupancy drops below threshold again, reset cooldown flag?
             // lock(_lastNotificationSent) { _lastNotificationSent.Remove(gymId); }
         }
    }

     public async Task<IEnumerable<HistoricalCapacityPoint>> GetHistoricalCapacityAsync(int gymId, DateTime startDate, DateTime endDate)
     {
        // Ensure dates are UTC
        startDate = startDate.ToUniversalTime().Date; // Start of day
        endDate = endDate.ToUniversalTime().Date.AddDays(1).AddTicks(-1); // End of day

        _logger.LogInformation("Fetching historical capacity for Gym {GymId} from {StartDate} to {EndDate}", gymId, startDate, endDate);

        // Get all relevant check-ins within the broader time window
        var relevantCheckIns = await _context.CheckIns
            .Where(c => c.IdGimnasio == gymId &&
                        c.HoraEntrada <= endDate && // Entered before or during the period
                        (c.HoraSalida == null || c.HoraSalida >= startDate)) // Exited during or after the period (or haven't exited)
            .OrderBy(c => c.HoraEntrada)
            .Select(c => new { c.HoraEntrada, c.HoraSalida }) // Select only needed fields
            .ToListAsync();

        var capacityData = new List<HistoricalCapacityPoint>();

        // Iterate through each hour in the requested range
        for (var dt = startDate; dt <= endDate; dt = dt.AddHours(1))
        {
            // Count how many people were checked in *at the start* of this hour
            int occupancyAtHourStart = relevantCheckIns.Count(c =>
                c.HoraEntrada <= dt &&                // Checked in before or exactly at the start of the hour
                (c.HoraSalida == null || c.HoraSalida > dt) // Haven't checked out OR checked out after the start of the hour
            );

            capacityData.Add(new HistoricalCapacityPoint
            {
                Timestamp = dt, // Represents the start of the hour interval
                Occupancy = occupancyAtHourStart
            });
        }

        _logger.LogInformation("Found {Count} historical capacity points for Gym {GymId}", capacityData.Count, gymId);
        return capacityData;
     }
}