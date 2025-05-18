using BookingManagementService.Data;
using BookingManagementService.Entities;
using BookingManagementService.Interfaces;
using BookingManagementService.Models;
using BookingManagementService.Models.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Add logging

namespace BookingManagementService.Services;

public class BookingService : IBookingService
{
    private readonly BookingDbContext _context;
    private readonly ILogger<BookingService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IRoutineEquipmentServiceClient _routineEquipmentClient;
     private readonly int _defaultExerciseDurationMinutes;
    private readonly int _machineTransitionGapMinutes;

    public BookingService(
        BookingDbContext context,
        ILogger<BookingService> logger,
        INotificationService notificationService,
        IRoutineEquipmentServiceClient routineEquipmentClient,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
        _routineEquipmentClient = routineEquipmentClient;
        _defaultExerciseDurationMinutes = configuration.GetValue<int>("BookingSettings:ExerciseDurationMinutes", 15);
        _machineTransitionGapMinutes = configuration.GetValue<int>("BookingSettings:MachineTransitionGapMinutes", 5);
    }
    
    private int EstimateExerciseDuration(ExternalRutinaDiaEjercicioDto exerciseDetails)
    {
        // Basic estimation: 3 sets * (1 min per set + 1 min rest) = 6 mins
        // More sophisticated: parse series/reps, add rest
        int sets = 3; // Default
        if (!string.IsNullOrEmpty(exerciseDetails.Series) && int.TryParse(exerciseDetails.Series.Split('x').First().Trim(), out int parsedSets))
        {
            sets = parsedSets;
        }
        int restSeconds = exerciseDetails.DescansoSegundos ?? 60;
        // Rough estimate: 30-60s per set execution + rest
        return (int)Math.Ceiling((double)sets * (60 + restSeconds) / 60.0); // Duration in minutes
        // Fallback if estimation is hard
        // return _defaultExerciseDurationMinutes;
    }

    private async Task<bool> IsMachineAvailableAsync(int machineId, DateTime slotStart, DateTime slotEnd)
    {
        return !await _context.ReservasMaquinas
            .AnyAsync(r => r.IdMaquina == machineId &&
                           r.Estado != "Cancelada" &&
                           r.FechaHoraInicio < slotEnd &&
                           r.FechaHoraFin > slotStart);
    }

    public async Task<RoutineDayAvailabilityResponse> ValidateRoutineDayAvailabilityAsync(ValidateRoutineDayAvailabilityRequest request)
    {
        _logger.LogInformation("Validating availability for Routine {RoutineId}, Day {DiaNumero}, Start: {StartDateTime}",
            request.RoutineId, request.DiaNumero, request.DesiredStartDateTime);

        var response = new RoutineDayAvailabilityResponse
        {
            OriginalRequestedStartTime = request.DesiredStartDateTime,
            IsOverallAvailable = true,
            Message = "Availability check in progress."
        };

        var routineData = await _routineEquipmentClient.GetRoutineByIdAsync(request.RoutineId);
        if (routineData == null || routineData.DiasEjercicios == null)
        {
            response.IsOverallAvailable = false;
            response.Message = "Routine details could not be fetched or routine is empty.";
            return response;
        }

        var exercisesForDay = routineData.DiasEjercicios
            .Where(de => de.DiaNumero == request.DiaNumero)
            .OrderBy(de => de.OrdenEnDia)
            .ToList();

        if (!exercisesForDay.Any())
        {
            response.IsOverallAvailable = true; // No exercises, so technically available
            response.Message = "No exercises found for the specified routine day.";
            return response;
        }

        DateTime currentSlotStartTime = request.DesiredStartDateTime.ToUniversalTime();
        DateTime? actualPossibleOverallStartTime = null;

        foreach (var exerciseInRoutine in exercisesForDay)
        {
            if (exerciseInRoutine.IdEjercicio <= 0 || string.IsNullOrEmpty(exerciseInRoutine.EjercicioNombre))
            {
                 response.ExerciseAvailabilities.Add(new ExerciseMachineAvailability {
                    ExerciseId = exerciseInRoutine.IdEjercicio, ExerciseName = "Unknown Exercise", IsAvailable = false, ReasonIfNotAvailable = "Invalid exercise data from routine service."
                });
                response.IsOverallAvailable = false;
                continue;
            }

            var exerciseDetails = await _routineEquipmentClient.GetExerciseByIdAsync(exerciseInRoutine.IdEjercicio);
            if (exerciseDetails == null || exerciseDetails.MaquinasAsociadas == null || !exerciseDetails.MaquinasAsociadas.Any())
            {
                response.ExerciseAvailabilities.Add(new ExerciseMachineAvailability {
                    ExerciseId = exerciseInRoutine.IdEjercicio, ExerciseName = exerciseInRoutine.EjercicioNombre, IsAvailable = false, ReasonIfNotAvailable = "No machines associated with this exercise or exercise details not found."
                });
                response.IsOverallAvailable = false;
                continue;
            }

            int durationMinutes = EstimateExerciseDuration(exerciseInRoutine);
            DateTime currentSlotEndTime = currentSlotStartTime.AddMinutes(durationMinutes);
            bool machineFoundForExercise = false;

            foreach (var machineOption in exerciseDetails.MaquinasAsociadas)
            {
                if (await IsMachineAvailableAsync(machineOption.IdMaquina, currentSlotStartTime, currentSlotEndTime))
                {
                    response.ExerciseAvailabilities.Add(new ExerciseMachineAvailability
                    {
                        ExerciseId = exerciseInRoutine.IdEjercicio,
                        ExerciseName = exerciseInRoutine.EjercicioNombre,
                        IsAvailable = true,
                        AvailableMachineId = machineOption.IdMaquina,
                        AvailableMachineName = machineOption.Nombre,
                        SuggestedStartTime = currentSlotStartTime
                    });
                    if (actualPossibleOverallStartTime == null) actualPossibleOverallStartTime = currentSlotStartTime;
                    currentSlotStartTime = currentSlotEndTime.AddMinutes(_machineTransitionGapMinutes);
                    machineFoundForExercise = true;
                    break;
                }
            }

            if (!machineFoundForExercise)
            {
                response.ExerciseAvailabilities.Add(new ExerciseMachineAvailability {
                    ExerciseId = exerciseInRoutine.IdEjercicio,
                    ExerciseName = exerciseInRoutine.EjercicioNombre,
                    IsAvailable = false,
                    ReasonIfNotAvailable = $"All associated machines booked from {currentSlotStartTime.ToLocalTime():t} to {currentSlotEndTime.ToLocalTime():t}."
                });
                response.IsOverallAvailable = false;                
                break;
            }
        }

        if (response.IsOverallAvailable)
        {
            response.Message = "All exercises for the routine day appear to be available at the specified start time or slightly adjusted.";
            response.ActualPossibleStartTime = actualPossibleOverallStartTime ?? request.DesiredStartDateTime;
        }
        else
        {
            if (string.IsNullOrEmpty(response.Message) || response.Message == "Availability check in progress.")
            {
                response.Message = "One or more machines required for the routine are not available. Please try a different start time or date.";
            }
        }
        return response;
    }

    public async Task<(RoutineDayBookingResponse? Response, string? ErrorMessage)> BookRoutineDayAsync(BookRoutineDayRequest request)
    {
        _logger.LogInformation("Attempting to book Routine {RoutineId}, Day {DiaNumero} for User {UserId}, starting at {StartDateTime}",
            request.RoutineId, request.DiaNumero, request.UserId, request.StartDateTime);

        // 1. Fetch Routine and Exercise details from RoutineEquipmentService
        var routineData = await _routineEquipmentClient.GetRoutineByIdAsync(request.RoutineId);
        if (routineData == null || routineData.DiasEjercicios == null)
        {
            return (null, "Could not retrieve routine details or routine is empty.");
        }

        var exercisesForDay = routineData.DiasEjercicios
            .Where(de => de.DiaNumero == request.DiaNumero)
            .OrderBy(de => de.OrdenEnDia)
            .ToList();

        if (!exercisesForDay.Any())
        {
            return (null, $"No exercises found for Day {request.DiaNumero} of Routine {request.RoutineId}.");
        }
        
        var bookedMachineSummaries = new List<RoutineDayBookingSummary>();
        DateTime currentPlannedStartTime = request.StartDateTime.ToUniversalTime();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var exerciseInRoutine in exercisesForDay)
            {
                if(exerciseInRoutine.IdEjercicio <= 0 || string.IsNullOrEmpty(exerciseInRoutine.EjercicioNombre)) {
                    _logger.LogWarning("Skipping invalid exercise data from routine service: {@ExerciseData}", exerciseInRoutine);
                    continue; // Skip malformed exercise data
                }

                var exerciseDetails = await _routineEquipmentClient.GetExerciseByIdAsync(exerciseInRoutine.IdEjercicio);
                if (exerciseDetails == null || exerciseDetails.MaquinasAsociadas == null || !exerciseDetails.MaquinasAsociadas.Any())
                {
                    await transaction.RollbackAsync();
                    return (null, $"Exercise '{exerciseInRoutine.EjercicioNombre}' (ID: {exerciseInRoutine.IdEjercicio}) has no associated machines or details not found.");
                }

                int durationMinutes = EstimateExerciseDuration(exerciseInRoutine);
                DateTime plannedEndTime = currentPlannedStartTime.AddMinutes(durationMinutes);
                bool bookedForThisExercise = false;
                int selectedMachineId = 0;
                string selectedMachineName = "Unknown";


                foreach (var machineOption in exerciseDetails.MaquinasAsociadas)
                {
                    if (await IsMachineAvailableAsync(machineOption.IdMaquina, currentPlannedStartTime, plannedEndTime))
                    {
                        var machineReservation = new ReservaMaquina
                        {
                            IdUsuario = request.UserId,
                            IdMaquina = machineOption.IdMaquina,
                            FechaHoraInicio = currentPlannedStartTime,
                            FechaHoraFin = plannedEndTime,
                            Estado = "Confirmada",
                            FechaCreacion = DateTime.UtcNow
                        };
                        _context.ReservasMaquinas.Add(machineReservation);
                        await _context.SaveChangesAsync();

                        selectedMachineId = machineOption.IdMaquina;
                        selectedMachineName = machineOption.Nombre ?? "Unknown Machine";

                        bookedMachineSummaries.Add(new RoutineDayBookingSummary
                        {
                            ExerciseId = exerciseInRoutine.IdEjercicio,
                            ExerciseName = exerciseInRoutine.EjercicioNombre,
                            MachineId = selectedMachineId,
                            MachineName = selectedMachineName,
                            ReservationId = machineReservation.IdReservaMaquina,
                            ReservedStartTime = currentPlannedStartTime,
                            ReservedEndTime = plannedEndTime
                        });

                        currentPlannedStartTime = plannedEndTime.AddMinutes(_machineTransitionGapMinutes); //agregar el tiempo del siguiente ejercicio
                        bookedForThisExercise = true;
                        _logger.LogInformation("Booked Machine {MachineId} for Exercise {ExerciseId} from {Start} to {End}",
                            selectedMachineId, exerciseInRoutine.IdEjercicio, machineReservation.FechaHoraInicio, machineReservation.FechaHoraFin);
                        break;
                    }
                }

                if (!bookedForThisExercise)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Could not find an available machine for Exercise '{ExerciseName}' (ID: {ExerciseId}) starting around {StartTime}",
                        exerciseInRoutine.EjercicioNombre, exerciseInRoutine.IdEjercicio, currentPlannedStartTime.ToLocalTime());
                    return (null, $"Failed to find an available machine for exercise '{exerciseInRoutine.EjercicioNombre}'. Routine booking cancelled.");
                }
            }

            await transaction.CommitAsync();
            _logger.LogInformation("Successfully booked all machines for Routine {RoutineId}, Day {DiaNumero} for User {UserId}",
                request.RoutineId, request.DiaNumero, request.UserId);

            return (new RoutineDayBookingResponse
            {
                UserId = request.UserId,
                RoutineId = request.RoutineId,
                DiaNumero = request.DiaNumero,
                Message = "All machines for the routine day have been successfully booked.",
                BookedMachines = bookedMachineSummaries
            }, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during routine day booking process for Routine {RoutineId}, User {UserId}.", request.RoutineId, request.UserId);
            return (null, "An unexpected error occurred during the booking process. All reservations have been rolled back.");
        }
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

            try
            {
                var notification = new NotificationRequest
                {
                    IdUsuario = reservation.IdUsuario,
                    Tipo = "ReservacionCreada",
                    Nombre = "Reservación de Máquina Confirmada",
                    Descripcion = $"Tu reservación para la máquina '{machine.Nombre}' el {reservation.FechaHoraInicio.ToLocalTime():D} a las {reservation.FechaHoraInicio.ToLocalTime():t} ha sido creada."
                };
                await _notificationService.SendNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation notification for machine reservation {ReservationId}", reservation.IdReservaMaquina);
            }
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

    public async Task<CancellationResult> CancelMachineReservationAsync(int reservationId, int requestingUserId)
    {
        _logger.LogInformation("Attempting to cancel machine reservation {ReservationId} by user {UserId}", reservationId, requestingUserId);

        var reservation = await _context.ReservasMaquinas
                                        // .Include(r => r.Usuario) // Optional: Include if needed for complex rules
                                        .FirstOrDefaultAsync(r => r.IdReservaMaquina == reservationId);

        if (reservation == null)
        {
            _logger.LogWarning("Machine reservation {ReservationId} not found for cancellation attempt by user {UserId}", reservationId, requestingUserId);
            return CancellationResult.NotFound;
        }

        if (reservation.Estado == "Cancelada")
        {
            _logger.LogInformation("Machine reservation {ReservationId} is already cancelled.", reservationId);
            return CancellationResult.Success;
        }

        if (reservation.Estado == "Completada" || reservation.Estado == "NoShow")
        {
            _logger.LogWarning("Cannot cancel machine reservation {ReservationId} because its status is {Status}", reservationId, reservation.Estado);
            return CancellationResult.Conflict; // Cannot cancel a completed or missed reservation
        }

        reservation.Estado = "Cancelada";
        reservation.Asistio = null; // Reset attendance status if applicable

        var machine = await _context.MaquinasEjercicio
                            .AsNoTracking() // Keep if you don't need to track changes to this entity
                            .FirstOrDefaultAsync(m => m.IdMaquina == reservation.IdMaquina);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully cancelled machine reservation {ReservationId} by user {UserId}", reservationId, requestingUserId);

            try {
                var notification = new NotificationRequest
                {
                    IdUsuario = reservation.IdUsuario,
                    Tipo = "ReservacionCancelada",
                    Nombre = "Reservación de Máquina Cancelada",
                    Descripcion = $"Tu reservación para la máquina '{machine?.Nombre}' el {reservation.FechaHoraInicio.ToLocalTime():D} a las {reservation.FechaHoraInicio.ToLocalTime():t} ha sido cancelada"
                };
                await _notificationService.SendNotificationAsync(notification);
                }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to send confirmation notification for machine reservation {ReservationId}", reservation.IdReservaMaquina);
            }

            return CancellationResult.Success;
        }
        catch (DbUpdateConcurrencyException ex)
        {
             _logger.LogError(ex, "Concurrency error cancelling machine reservation {ReservationId}", reservationId);
            return CancellationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cancellation for machine reservation {ReservationId}", reservationId);
            return CancellationResult.Conflict;
        }
    }

    public async Task<CancellationResult> CancelTrainerReservationAsync(int reservationId, int requestingUserId, bool isAdmin)
    {
         _logger.LogInformation("Attempting to cancel trainer reservation {ReservationId} by user {UserId}", reservationId, requestingUserId);

        var reservation = await _context.ReservasEntrenador
                                    .FirstOrDefaultAsync(r => r.IdReservaEntrenador == reservationId);

         if (reservation == null)
        {
            _logger.LogWarning("Trainer reservation {ReservationId} not found for cancellation attempt by user {UserId}", reservationId, requestingUserId);
            return CancellationResult.NotFound;
        }

        if (reservation.IdCliente != requestingUserId && reservation.IdEntrenador != requestingUserId && !isAdmin)
        {
             _logger.LogWarning("User {UserId} forbidden to cancel trainer reservation {ReservationId} (Client: {ClientId}, Trainer: {TrainerId})",
                requestingUserId, reservationId, reservation.IdCliente, reservation.IdEntrenador);
             return CancellationResult.Forbidden;
        }

        // Business Rule Check:
        if (reservation.Estado == "Cancelada")
        {
            _logger.LogInformation("Trainer reservation {ReservationId} is already cancelled.", reservationId);
             return CancellationResult.Success;
        }
        if (reservation.Estado == "Completada" || reservation.Estado == "NoShowCliente" || reservation.Estado == "NoShowEntrenador")
        {
             _logger.LogWarning("Cannot cancel trainer reservation {ReservationId} because its status is {Status}", reservationId, reservation.Estado);
            return CancellationResult.Conflict;
        }

         // Perform Cancellation
        reservation.Estado = "Cancelada";
        reservation.AsistioCliente = null;
        reservation.AsistioEntrenador = null;

        try
        {
            await _context.SaveChangesAsync();
             _logger.LogInformation("Successfully cancelled trainer reservation {ReservationId} by user {UserId}", reservationId, requestingUserId);
            return CancellationResult.Success;
        }
        catch (DbUpdateConcurrencyException ex) // Handle potential race conditions
        {
             _logger.LogError(ex, "Concurrency error cancelling trainer reservation {ReservationId}", reservationId);
            return CancellationResult.Conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cancellation for trainer reservation {ReservationId}", reservationId);
            return CancellationResult.Conflict;
        }
    }
    
    public async Task<CancellationResult> CancelClassRegistrationAsync(int registrationId, int requestingUserId, bool isAdmin)
    {
        _logger.LogInformation("Attempting to cancel class registration {RegistrationId} by user {UserId}", registrationId, requestingUserId);

        var registration = await _context.InscripcionesClases
                                        .Include(ic => ic.Clase) // Include Clase if time-based rules apply
                                        .FirstOrDefaultAsync(ic => ic.IdInscripcion == registrationId);

        if (registration == null)
        {
            _logger.LogWarning("Class registration {RegistrationId} not found for cancellation attempt by user {UserId}", registrationId, requestingUserId);
            return CancellationResult.NotFound;
        }

        if (registration.IdUsuario != requestingUserId && !isAdmin)
        {
            _logger.LogWarning("User {UserId} forbidden to cancel class registration {RegistrationId} belonging to user {OwnerId}",
                requestingUserId, registrationId, registration.IdUsuario);
            return CancellationResult.Forbidden;
        }

        // Business Rule Check:
        //preventing cancellation if class already started/finished
        if (registration.Clase?.FechaHoraInicio <= DateTime.UtcNow) 
        { 
            return CancellationResult.Conflict;
        }

        try
        {
            _context.InscripcionesClases.Remove(registration);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully cancelled (removed) class registration {RegistrationId} by user {UserId}", registrationId, requestingUserId);
            return CancellationResult.Success;
        }
         catch (DbUpdateConcurrencyException ex)
        {
             _logger.LogError(ex, "Concurrency error cancelling class registration {RegistrationId}", registrationId);
             return CancellationResult.NotFound; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cancellation for class registration {RegistrationId}", registrationId);
            return CancellationResult.Conflict;
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