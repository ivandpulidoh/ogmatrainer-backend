using CapacityControlService.Data;
using CapacityControlService.Entities;
using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CapacityControlService.Services;

public class AttendanceService : IAttendanceService
{
    private readonly CapacityDbContext _context;
    private readonly ICapacityService _capacityService; // Inject capacity service
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(CapacityDbContext context, ICapacityService capacityService, ILogger<AttendanceService> logger)
    {
        _context = context;
        _capacityService = capacityService;
        _logger = logger;
    }

    public async Task<(CheckInResponse? Response, string? ErrorMessage, bool CapacityReached)> CheckInAsync(CheckInRequest request)
    {
        _logger.LogInformation("Attempting check-in for User {UserId} at Gym {GymId}", request.UserId, request.GymId);

        // Validate Gym
        var gym = await _context.Gimnasios.FindAsync(request.GymId);
        if (gym == null) return (null, "Gym not found.", false);

        var userExists = await _context.Usuarios.AnyAsync(u => u.IdUsuario == request.UserId);
        if (!userExists) return (null, "User not found.", false);

        // Check if user is already checked in at THIS gym
        var existingCheckin = await _context.CheckIns
            .FirstOrDefaultAsync(c => c.IdUsuario == request.UserId && c.IdGimnasio == request.GymId && c.HoraSalida == null);
        if (existingCheckin != null)
        {
            _logger.LogWarning("User {UserId} already checked in at Gym {GymId} (CheckinId: {CheckinId})", request.UserId, request.GymId, existingCheckin.IdCheckin);
            // Optionally return existing checkin info? Or error? Let's return error for now.
            return (null, "User is already checked in at this gym.", false);
        }

        // --- Capacity Check ---
        int currentOccupancy = await _context.CheckIns.CountAsync(c => c.IdGimnasio == request.GymId && c.HoraSalida == null);
        if (currentOccupancy >= gym.CapacidadMaxima)
        {
            _logger.LogWarning("Gym {GymId} is at full capacity ({Current}/{Max}). Check-in denied for User {UserId}",
                request.GymId, currentOccupancy, gym.CapacidadMaxima, request.UserId);
            return (null, "Gym is currently at full capacity.", true); // Signal capacity reached
        }
        // -----------------------

        var newCheckIn = new CheckIn
        {
            IdUsuario = request.UserId,
            IdGimnasio = request.GymId,
            HoraEntrada = DateTime.UtcNow
        };

        try
        {
            _context.CheckIns.Add(newCheckIn);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Check-in successful for User {UserId} at Gym {GymId}. CheckinId: {CheckinId}", request.UserId, request.GymId, newCheckIn.IdCheckin);

             // Check capacity AFTER successful check-in for notification threshold
             await _capacityService.CheckCapacityAndNotifyAsync(request.GymId, currentOccupancy + 1);


            var response = new CheckInResponse
            {
                CheckInId = newCheckIn.IdCheckin,
                UserId = newCheckIn.IdUsuario,
                GymId = newCheckIn.IdGimnasio,
                EntryTime = newCheckIn.HoraEntrada,
                RequiresSymptomForm = true // Always require form after check-in
            };
            return (response, null, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving check-in for User {UserId} at Gym {GymId}", request.UserId, request.GymId);
            return (null, "Failed to save check-in record.", false);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> CheckOutAsync(CheckOutRequest request)
    {
         _logger.LogInformation("Attempting check-out for User {UserId} at Gym {GymId}", request.UserId, request.GymId);

        // Find the latest open check-in for this user at this gym
        var checkInToClose = await _context.CheckIns
            .Where(c => c.IdUsuario == request.UserId && c.IdGimnasio == request.GymId && c.HoraSalida == null)
            .OrderByDescending(c => c.HoraEntrada) // Get the most recent one
            .FirstOrDefaultAsync();

        if (checkInToClose == null)
        {
             _logger.LogWarning("No active check-in found for User {UserId} at Gym {GymId} to check out.", request.UserId, request.GymId);
            return (false, "No active check-in found for this user at the specified gym.");
        }

        checkInToClose.HoraSalida = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
             _logger.LogInformation("Check-out successful for User {UserId} at Gym {GymId}. CheckinId: {CheckinId}", request.UserId, request.GymId, checkInToClose.IdCheckin);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving check-out for User {UserId} at Gym {GymId}, CheckinId {CheckinId}", request.UserId, request.GymId, checkInToClose.IdCheckin);
            return (false, "Failed to save check-out time.");
        }
    }
}