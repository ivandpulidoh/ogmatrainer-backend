using BookingManagementService.Data;
using BookingManagementService.Entities;
using BookingManagementService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Add logging

namespace BookingManagementService.Services;

public class BookingService : IBookingService
{
    private readonly BookingDbContext _context;
    private readonly ILogger<BookingService> _logger;

    public BookingService(BookingDbContext context, ILogger<BookingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, int? ReservationId, string? ErrorMessage)> CreateMachineReservationAsync(CreateMachineReservationRequest request)
    {
        if (!request.IsValid())
        {
            return (false, null, "Invalid request data.");
        }

        // 1. Check if machine exists and is reservable
        var machine = await _context.MaquinasEjercicio
                                    .AsNoTracking() // Read-only check
                                    .FirstOrDefaultAsync(m => m.IdMaquina == request.IdMaquina && m.Reservable && m.Estado == "Disponible");
        if (machine == null)
        {
            return (false, null, "Machine not found, not reservable, or not available.");
        }

        // 2. Check for user existence (optional, depends on auth)
        var userExists = await _context.Usuarios.AnyAsync(u => u.IdUsuario == request.IdUsuario && u.Activo);
        if (!userExists)
        {
            return (false, null, "User not found or inactive.");
        }

        // 3. Check for overlapping reservations for this machine
        var overlapping = await _context.ReservasMaquinas
            .AnyAsync(r => r.IdMaquina == request.IdMaquina &&
                           r.Estado != "Cancelada" && // Don't check against cancelled
                           r.FechaHoraInicio < request.FechaHoraFin &&
                           r.FechaHoraFin > request.FechaHoraInicio);

        if (overlapping)
        {
            return (false, null, "Machine is already booked during this time slot.");
        }

        // 4. Create and save the reservation
        var reservation = new ReservaMaquina
        {
            IdUsuario = request.IdUsuario,
            IdMaquina = request.IdMaquina,
            FechaHoraInicio = request.FechaHoraInicio.ToUniversalTime(), // Store in UTC
            FechaHoraFin = request.FechaHoraFin.ToUniversalTime(),     // Store in UTC
            FechaCreacion = DateTime.UtcNow,
            Estado = "Confirmada", // Default state
            Asistio = null // Default attendance
        };

        try
        {
            _context.ReservasMaquinas.Add(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Machine reservation {ReservationId} created for user {UserId}", reservation.IdReservaMaquina, reservation.IdUsuario);
            return (true, reservation.IdReservaMaquina, null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to create machine reservation for user {UserId}", request.IdUsuario);
            return (false, null, "Failed to save reservation. Please try again.");
        }
    }

    public async Task<(bool Success, int? ReservationId, string? ErrorMessage)> CreateTrainerReservationAsync(CreateTrainerReservationRequest request)
    {
        if (!request.IsValid())
        {
            return (false, null, "Invalid request data.");
        }

        // 1. Check Client/Trainer existence (simplified, add Role checks if needed)
        var clientExists = await _context.Usuarios.AnyAsync(u => u.IdUsuario == request.IdCliente && u.Activo);
        var trainerExists = await _context.Usuarios.AnyAsync(u => u.IdUsuario == request.IdEntrenador && u.Activo); // Add role check later

        if (!clientExists || !trainerExists)
        {
            return (false, null, "Client or Trainer not found or inactive.");
        }

        // 2. Check Trainer availability (overlapping reservations)
        var trainerOverlapping = await _context.ReservasEntrenador
           .AnyAsync(r => r.IdEntrenador == request.IdEntrenador &&
                          r.Estado != "Cancelada" && // Don't check against cancelled
                          r.FechaHoraInicio < request.FechaHoraFin &&
                          r.FechaHoraFin > request.FechaHoraInicio);

        if (trainerOverlapping)
        {
            return (false, null, "Trainer is already booked during this time slot.");
        }

        // 3. Check Space availability if IdEspacio is provided (implement similarly if needed)

        // 4. Create and save
        var reservation = new ReservaEntrenador
        {
            IdCliente = request.IdCliente,
            IdEntrenador = request.IdEntrenador,
            IdEspacio = request.IdEspacio,
            FechaHoraInicio = request.FechaHoraInicio.ToUniversalTime(),
            FechaHoraFin = request.FechaHoraFin.ToUniversalTime(),
            FechaCreacion = DateTime.UtcNow,
            Estado = "Confirmada",
            AsistioCliente = null,
            AsistioEntrenador = null
        };

        try
        {
            _context.ReservasEntrenador.Add(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Trainer reservation {ReservationId} created for client {ClientId} with trainer {TrainerId}", reservation.IdReservaEntrenador, reservation.IdCliente, reservation.IdEntrenador);

            return (true, reservation.IdReservaEntrenador, null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to create trainer reservation for client {ClientId}", request.IdCliente);
            return (false, null, "Failed to save reservation. Please try again.");
        }
    }

    public async Task<(bool Success, int? RegistrationId, string? ErrorMessage)> RegisterForClassAsync(int classId, int userId)
    {
        // 1. Check User exists
        var userExists = await _context.Usuarios.AnyAsync(u => u.IdUsuario == userId && u.Activo);
        if (!userExists)
        {
            return (false, null, "User not found or inactive.");
        }

        // 2. Check Class exists, is active, is 'EnVivo' (if capacity matters), and has capacity
        var clase = await _context.Clases
                                 .FirstOrDefaultAsync(c => c.IdClase == classId && c.Activa);

        if (clase == null)
        {
            return (false, null, "Class not found or is inactive.");
        }

        // 3. Check if already registered
        var alreadyRegistered = await _context.InscripcionesClases
                                       .AnyAsync(ic => ic.IdClase == classId && ic.IdUsuario == userId);
        if (alreadyRegistered)
        {
            return (false, null, "User is already registered for this class.");
        }

        // 4. Check Capacity (only for relevant class types, e.g., 'EnVivo')
        if (clase.Tipo == "EnVivo" && clase.CapacidadMaxima.HasValue)
        {
            var currentEnrollment = await _context.InscripcionesClases.CountAsync(ic => ic.IdClase == classId);
            if (currentEnrollment >= clase.CapacidadMaxima.Value)
            {
                return (false, null, "Class is full.");
            }
        }

        // 5. Create Registration
        var registration = new InscripcionClase
        {
            IdUsuario = userId,
            IdClase = classId,
            FechaInscripcion = DateTime.UtcNow,
            Asistio = null
        };

        try
        {
            _context.InscripcionesClases.Add(registration);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} registered for class {ClassId}", userId, classId);
            return (true, registration.IdInscripcion, null);
        }
        catch (DbUpdateException ex)
        {
            // Check for unique constraint violation (user might have registered concurrently)
            if (ex.InnerException?.Message.Contains("UNIQUE KEY constraint") ?? false) // Check specific error in real app
            {
                return (false, null, "User is already registered for this class (concurrent registration detected).");
            }
            _logger.LogError(ex, "Failed to register user {UserId} for class {ClassId}", userId, classId);
            return (false, null, "Failed to save registration.");
        }
    }

    // --- Retrieval Methods ---

    public async Task<IEnumerable<BookingDto>> GetUserBookingsForDayAsync(int userId, DateOnly date)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var machineBookings = await _context.ReservasMaquinas
            .Where(r => r.IdUsuario == userId && r.FechaHoraInicio >= startOfDay && r.FechaHoraInicio <= endOfDay)
            .Include(r => r.MaquinaEjercicio) // Include related data
            .Select(r => new BookingDto
            {
                ReservationId = r.IdReservaMaquina,
                ReservationType = "Machine",
                UserId = r.IdUsuario,
                ItemId = r.IdMaquina,
                ItemName = r.MaquinaEjercicio.Nombre,
                StartTime = r.FechaHoraInicio,
                EndTime = r.FechaHoraFin,
                Status = r.Estado,
                Attended = r.Asistio
            })
             .ToListAsync();

        var trainerBookings = await _context.ReservasEntrenador
            .Where(r => r.IdCliente == userId && r.FechaHoraInicio >= startOfDay && r.FechaHoraInicio <= endOfDay)
            .Include(r => r.Entrenador) // Include trainer info
             .Select(r => new BookingDto
             {
                 ReservationId = r.IdReservaEntrenador,
                 ReservationType = "Trainer",
                 UserId = r.IdCliente, // User is the client
                 ItemId = r.IdEntrenador,
                 ItemName = r.Entrenador.Nombre + " " + r.Entrenador.Apellido, // Get trainer name
                 StartTime = r.FechaHoraInicio,
                 EndTime = r.FechaHoraFin,
                 Status = r.Estado,
                 Attended = r.AsistioCliente // Show client attendance
             })
             .ToListAsync();

        var classRegistrationsData = await _context.InscripcionesClases
       .Where(r => r.IdUsuario == userId &&
                   r.Clase.FechaHoraInicio != null && // Ensure there's a start date for comparison
                   r.Clase.FechaHoraInicio >= startOfDay &&
                   r.Clase.FechaHoraInicio <= endOfDay)
       .Include(r => r.Clase) // Still need class info
       .Select(r => new // Select only the raw data needed
       {
           RegistrationId = r.IdInscripcion,
           UserId = r.IdUsuario,
           ClassId = r.IdClase,
           ClassName = r.Clase.NombreClase,
           StartTime = r.Clase.FechaHoraInicio, // Get nullable DateTime
           Duration = r.Clase.DuracionMinutos, // Get nullable int
           Attended = r.Asistio
       })
       .ToListAsync(); // *** Execute DB query here ***

        var classBookings = await _context.InscripcionesClases
           .Where(r => r.IdUsuario == userId && r.Clase.FechaHoraInicio >= startOfDay && r.Clase.FechaHoraInicio <= endOfDay)
           .Include(r => r.Clase)
            .Select(r => new BookingDto
            {
                ReservationId = r.IdInscripcion, // Use registration ID
                ReservationType = "Class",
                UserId = r.IdUsuario,
                ItemId = r.IdClase,
                ItemName = r.Clase.NombreClase,
                StartTime = r.Clase.FechaHoraInicio ?? DateTime.MinValue, // Handle potential null (though should exist for check)
                EndTime = r.Clase.FechaHoraInicio != null
                    ? r.Clase.FechaHoraInicio.Value.AddMinutes(r.Clase.DuracionMinutos)
                    : DateTime.MinValue,
                Status = "Registered", // Or derive from Clase status if needed
                Attended = r.Asistio
            })
            .ToListAsync();

        return machineBookings.Concat(trainerBookings).Concat(classBookings).OrderBy(b => b.StartTime);
    }

    public async Task<IEnumerable<BookingDto>> GetAllBookingsForDayAsync(DateOnly date)
    {
        // Similar logic to GetUserBookingsForDayAsync but without the userId filter
        var startOfDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        // Combine queries for Machines, Trainers, Classes for the given day
        // ... (Implementation similar to GetUserBookingsForDayAsync, removing userId filters)
        // Remember to include necessary details like user names if needed for an admin view

        // Example for machines:
        var machineBookings = await _context.ReservasMaquinas
          .Where(r => r.FechaHoraInicio >= startOfDay && r.FechaHoraInicio <= endOfDay)
          .Include(r => r.MaquinaEjercicio)
          .Include(r => r.Usuario) // Include user details
          .Select(r => new BookingDto
          {
              ReservationId = r.IdReservaMaquina,
              ReservationType = "Machine",
              UserId = r.IdUsuario,
              // Add UserName = r.Usuario.Nombre + " " + r.Usuario.Apellido, if needed
              ItemId = r.IdMaquina,
              ItemName = r.MaquinaEjercicio.Nombre,
              StartTime = r.FechaHoraInicio,
              EndTime = r.FechaHoraFin,
              Status = r.Estado,
              Attended = r.Asistio
          })
           .ToListAsync();

        // Add similar queries for Trainer and Class reservations/registrations

        // Combine and return
        // return machineBookings.Concat(trainerBookings).Concat(classBookings).OrderBy(b => b.StartTime);
        return machineBookings.OrderBy(b => b.StartTime); // Return just machines for brevity here
    }


    // --- Attendance Update Methods ---
    // These might be called by the background service or other triggers

    public async Task UpdateMachineAttendanceAsync(int reservationId, bool attended)
    {
        var reservation = await _context.ReservasMaquinas.FindAsync(reservationId);
        if (reservation != null)
        {
            reservation.Asistio = attended;
            // Optionally update status e.g. to "Completada" if attended, keep as is or change if not
            if (!attended && reservation.Estado == "Confirmada") // Only mark NoShow if it wasn't cancelled etc.
            {
                reservation.Estado = "NoShow";
            }
            else if (attended && reservation.Estado == "Confirmada") // Mark completed if they attended
            {
                reservation.Estado = "Completada";
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated attendance for machine reservation {ReservationId} to {Attended}", reservationId, attended);
        }
        else
        {
            _logger.LogWarning("Attempted to update attendance for non-existent machine reservation {ReservationId}", reservationId);
        }
    }

    public async Task UpdateTrainerAttendanceAsync(int reservationId, bool? clientAttended, bool? trainerAttended)
    {
        var reservation = await _context.ReservasEntrenador.FindAsync(reservationId);
        if (reservation != null)
        {
            if (clientAttended.HasValue) reservation.AsistioCliente = clientAttended.Value;
            if (trainerAttended.HasValue) reservation.AsistioEntrenador = trainerAttended.Value;

            // Add logic to update Status based on attendance (e.g., NoShowCliente, Completed)
            if (reservation.Estado == "Confirmada")
            {
                if (clientAttended == false) reservation.Estado = "NoShowCliente";
                else if (trainerAttended == false) reservation.Estado = "NoShowEntrenador"; // Or handle combined no-shows
                else if (clientAttended == true && trainerAttended == true) reservation.Estado = "Completada";
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated attendance for trainer reservation {ReservationId}", reservationId);
        }
        else
        {
            _logger.LogWarning("Attempted to update attendance for non-existent trainer reservation {ReservationId}", reservationId);
        }
    }

    public async Task UpdateClassAttendanceAsync(int registrationId, bool attended)
    {
        var registration = await _context.InscripcionesClases.FindAsync(registrationId);
        if (registration != null)
        {
            registration.Asistio = attended;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated attendance for class registration {RegistrationId} to {Attended}", registrationId, attended);
        }
        else
        {
            _logger.LogWarning("Attempted to update attendance for non-existent class registration {RegistrationId}", registrationId);
        }
    }
}