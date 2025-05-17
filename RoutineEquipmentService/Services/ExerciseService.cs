using Microsoft.EntityFrameworkCore;
using RoutineEquipmentService.Data;
using RoutineEquipmentService.Dtos;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Models; // Ensure DTOs are in this namespace or adjust
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Services;

public class ExerciseService : IExerciseService
{
    private readonly RoutineEquipmentDbContext _context;
    private readonly ILogger<ExerciseService> _logger;

    public ExerciseService(RoutineEquipmentDbContext context, ILogger<ExerciseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(EjercicioResponse? Exercise, string? ErrorMessage)> CreateExerciseAsync(CreateEjercicioRequest request, int? creatorUserId)
    {
        _logger.LogInformation("Creating new exercise: {ExerciseName} by User: {CreatorId}", request.Nombre, creatorUserId ?? 0);

        // Check for existing exercise with the same name (unique constraint)
        if (await _context.Ejercicios.AnyAsync(e => e.Nombre == request.Nombre))
        {
            _logger.LogWarning("Exercise with name {ExerciseName} already exists.", request.Nombre);
            return (null, $"An exercise with the name '{request.Nombre}' already exists.");
        }

        if (request.MaquinasRequeridasIds != null && request.MaquinasRequeridasIds.Any())
        {
            var distinctRequestedMachineIds = request.MaquinasRequeridasIds.Distinct().ToList();
            var existingMachineIds = await _context.MaquinasEjercicio
                                               .Where(m => distinctRequestedMachineIds.Contains(m.IdMaquina))
                                               .Select(m => m.IdMaquina)
                                               .ToListAsync();

            var missingMachineIds = distinctRequestedMachineIds.Except(existingMachineIds).ToList();
            if (missingMachineIds.Any())
            {
                _logger.LogWarning("Cannot create exercise. Invalid or non-existent machine IDs provided: {MissingIds}", string.Join(", ", missingMachineIds));
                return (null, $"The following machine IDs are invalid or do not exist: {string.Join(", ", missingMachineIds)}");
            }
        }

        var newExercise = new Ejercicio
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            MusculoObjetivo = request.MusculoObjetivo,
            UrlVideoDemostracion = request.UrlVideoDemostracion,
            IdCreador = creatorUserId
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Ejercicios.Add(newExercise);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exercise {ExerciseName} created successfully with ID {ExerciseId}.", newExercise.Nombre, newExercise.IdEjercicio);

            if (request.MaquinasRequeridasIds != null && request.MaquinasRequeridasIds.Any())
            {
                foreach (var machineId in request.MaquinasRequeridasIds.Distinct())
                {
                    var ejercicioMaquina = new EjercicioMaquina
                    {
                        IdEjercicio = newExercise.IdEjercicio,
                        IdMaquina = machineId
                    };
                    _context.EjercicioMaquinas.Add(ejercicioMaquina);
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Linked exercise ID {ExerciseId} to {Count} machines.", newExercise.IdEjercicio, request.MaquinasRequeridasIds.Distinct().Count());
            }

            await transaction.CommitAsync(); // Commit transaction if all successful

            _logger.LogInformation("Exercise {ExerciseName} and machine links created successfully with ID {ExerciseId}.", newExercise.Nombre, newExercise.IdEjercicio);
            return (MapToResponse(newExercise), null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating exercise {ExerciseName}.", request.Nombre);
            return (null, "Failed to create exercise due to a database error. Check for unique constraints.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error creating exercise {ExerciseName}.", request.Nombre);
            return (null, "An unexpected error occurred while creating the exercise.");
        }
    }

    public async Task<EjercicioResponse?> GetExerciseByIdAsync(int exerciseId)
    {
        _logger.LogInformation("Fetching exercise by ID: {ExerciseId} with associated machines.", exerciseId);
        var exercise = await _context.Ejercicios
            .AsNoTracking()
            .Include(e => e.MaquinasRequeridas)
                .ThenInclude(em => em.MaquinaEjercicio)
            .FirstOrDefaultAsync(e => e.IdEjercicio == exerciseId);

        return exercise == null ? null : MapToResponse(exercise);
    }

    public async Task<IEnumerable<EjercicioResponse>> GetAllExercisesAsync()
    {
        _logger.LogInformation("Fetching all exercises with their associated machines.");
        var exercises = await _context.Ejercicios
            .AsNoTracking()
            .Include(e => e.MaquinasRequeridas)
                .ThenInclude(em => em.MaquinaEjercicio)
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        return exercises.Select(e => MapToResponse(e));
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateExerciseAsync(int exerciseId, UpdateEjercicioRequest request, int? updaterUserId)
    {
        _logger.LogInformation("Updating exercise with ID {ExerciseId} by User: {UpdaterId}", exerciseId, updaterUserId ?? 0);
        var exercise = await _context.Ejercicios
                                .Include(e => e.MaquinasRequeridas) // Load existing associations
                                .FirstOrDefaultAsync(e => e.IdEjercicio == exerciseId);

        if (exercise == null)
        {
            _logger.LogWarning("Exercise with ID {ExerciseId} not found for update.", exerciseId);
            return (false, "Exercise not found.");
        }

        if (!string.IsNullOrEmpty(request.Nombre) && request.Nombre != exercise.Nombre)
        {
            if (await _context.Ejercicios.AnyAsync(e => e.IdEjercicio != exerciseId && e.Nombre == request.Nombre))
            {
                _logger.LogWarning("Another exercise with name {ExerciseName} already exists. Update failed for ID {ExerciseId}", request.Nombre, exerciseId);
                return (false, $"An exercise with the name '{request.Nombre}' already exists.");
            }
            exercise.Nombre = request.Nombre;
        }
        exercise.Descripcion = request.Descripcion;
        exercise.MusculoObjetivo = request.MusculoObjetivo;
        exercise.UrlVideoDemostracion = request.UrlVideoDemostracion;

        // --- Handle Machine Associations Update ---
        if (request.MaquinasRequeridasIds != null) // If client provides the list, update associations
        {
            // Validate new machine IDs
            var distinctRequestedMachineIds = request.MaquinasRequeridasIds.Distinct().ToList();
            var existingValidMachineIdsInRequest = await _context.MaquinasEjercicio
                                               .Where(m => distinctRequestedMachineIds.Contains(m.IdMaquina))
                                               .Select(m => m.IdMaquina)
                                               .ToListAsync();
            var missingMachineIds = distinctRequestedMachineIds.Except(existingValidMachineIdsInRequest).ToList();
            if (missingMachineIds.Any())
            {
                _logger.LogWarning("Cannot update exercise. Invalid or non-existent machine IDs provided for association: {MissingIds}", string.Join(", ", missingMachineIds));
                return (false, $"The following machine IDs for association are invalid or do not exist: {string.Join(", ", missingMachineIds)}");
            }

            // Remove old associations not in the new list
            var machineIdsToRemove = exercise.MaquinasRequeridas
                                          .Select(em => em.IdMaquina)
                                          .Except(existingValidMachineIdsInRequest)
                                          .ToList();
            if (machineIdsToRemove.Any())
            {
                var associationsToRemove = exercise.MaquinasRequeridas
                                                .Where(em => machineIdsToRemove.Contains(em.IdMaquina))
                                                .ToList();
                _context.EjercicioMaquinas.RemoveRange(associationsToRemove);
            }

            // Add new associations
            var currentAssociatedMachineIds = exercise.MaquinasRequeridas.Select(em => em.IdMaquina).ToList();
            var machineIdsToAdd = existingValidMachineIdsInRequest.Except(currentAssociatedMachineIds).ToList();
            if (machineIdsToAdd.Any())
            {
                foreach (var machineId in machineIdsToAdd)
                {
                    _context.EjercicioMaquinas.Add(new EjercicioMaquina { IdEjercicio = exerciseId, IdMaquina = machineId });
                }
            }
        }
        
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exercise with ID {ExerciseId} updated successfully.", exerciseId);
            return (true, null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating exercise {ExerciseId}.", exerciseId);
            return (false, "Failed to update exercise due to a database error.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteExerciseAsync(int exerciseId, int? deleterUserId)
    {
         _logger.LogInformation("Deleting exercise with ID {ExerciseId} by User: {DeleterId}", exerciseId, deleterUserId ?? 0);
        var exercise = await _context.Ejercicios.FindAsync(exerciseId);
        if (exercise == null)
        {
            _logger.LogWarning("Exercise with ID {ExerciseId} not found for deletion.", exerciseId);
            return (false, "Exercise not found.");
        }

        try
        {            
            _context.Ejercicios.Remove(exercise);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exercise with ID {ExerciseId} deleted successfully.", exerciseId);
            return (true, null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting exercise {ExerciseId}. It might be in use.", exerciseId);
            return (false, "Failed to delete exercise. It might be in use in existing routines or other records not handled by cascade.");
        }
    }

    private static EjercicioResponse MapToResponse(Ejercicio exercise)
    {
        return new EjercicioResponse
        {
            IdEjercicio = exercise.IdEjercicio,
            Nombre = exercise.Nombre,
            Descripcion = exercise.Descripcion,
            MusculoObjetivo = exercise.MusculoObjetivo,
            UrlVideoDemostracion = exercise.UrlVideoDemostracion,
            IdCreador = exercise.IdCreador,
            MaquinasAsociadas = exercise.MaquinasRequeridas?
                .Select(em => em.MaquinaEjercicio)
                .Where(m => m != null)
                .Select(m => new SimpleMaquinaDto
                {
                    IdMaquina = m!.IdMaquina,
                    Nombre = m.Nombre,
                    UrlImagen = m.UrlImagen
                }).ToList() ?? new List<SimpleMaquinaDto>()
        };
    }
}