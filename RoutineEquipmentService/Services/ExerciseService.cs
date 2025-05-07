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

        var newExercise = new Ejercicio
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            MusculoObjetivo = request.MusculoObjetivo,
            UrlVideoDemostracion = request.UrlVideoDemostracion,
            IdCreador = creatorUserId
        };

        try
        {
            _context.Ejercicios.Add(newExercise);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exercise {ExerciseName} created successfully with ID {ExerciseId}.", newExercise.Nombre, newExercise.IdEjercicio);
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
        var exercise = await _context.Ejercicios.AsNoTracking().FirstOrDefaultAsync(e => e.IdEjercicio == exerciseId);
        return exercise == null ? null : MapToResponse(exercise);
    }

    public async Task<IEnumerable<EjercicioResponse>> GetAllExercisesAsync()
    {
        return await _context.Ejercicios
            .AsNoTracking()
            .Select(e => MapToResponse(e))
            .ToListAsync();
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateExerciseAsync(int exerciseId, UpdateEjercicioRequest request, int? updaterUserId)
    {
        _logger.LogInformation("Updating exercise with ID {ExerciseId} by User: {UpdaterId}", exerciseId, updaterUserId ?? 0);
        var exercise = await _context.Ejercicios.FindAsync(exerciseId);
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

        // Update other properties (allow setting to null or empty if desired)
        exercise.Descripcion = request.Descripcion; // Update even if null/empty to clear it
        exercise.MusculoObjetivo = request.MusculoObjetivo;
        exercise.UrlVideoDemostracion = request.UrlVideoDemostracion;
        // IdCreador is usually not updated unless by a super admin or specific logic

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exercise with ID {ExerciseId} updated successfully.", exerciseId);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating exercise {ExerciseId}.", exerciseId);
            return (false, "Failed to update exercise due to a concurrency conflict.");
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
        // Catch DbUpdateException for FK constraint violations (e.g., if exercise is in use in RutinaDiaEjercicios)
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting exercise {ExerciseId}. It might be in use.", exerciseId);
            return (false, "Failed to delete exercise. It might be in use in existing routines.");
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
            IdCreador = exercise.IdCreador
        };
    }
}