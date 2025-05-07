using Microsoft.EntityFrameworkCore;
using RoutineEquipmentService.Data;
using RoutineEquipmentService.Dtos;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Services;

public class RoutineService : IRoutineService
{
    private readonly RoutineEquipmentDbContext _context;
    private readonly ILogger<RoutineService> _logger;

    public RoutineService(RoutineEquipmentDbContext context, ILogger<RoutineService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(RutinaResponse? Routine, string? ErrorMessage)> CreateRoutineAsync(CreateRutinaRequest request, int creatorUserId)
    {
        _logger.LogInformation("Creating new routine: {RoutineName} by User: {CreatorId}", request.NombreRutina, creatorUserId);

        // Validate exercise IDs exist
        var exerciseIdsInRequest = request.DiasEjercicios.Select(de => de.IdEjercicio).Distinct().ToList();
        var existingExerciseIds = await _context.Ejercicios
                                            .Where(e => exerciseIdsInRequest.Contains(e.IdEjercicio))
                                            .Select(e => e.IdEjercicio)
                                            .ToListAsync();

        var missingExerciseIds = exerciseIdsInRequest.Except(existingExerciseIds).ToList();
        if (missingExerciseIds.Any())
        {
            _logger.LogWarning("Cannot create routine. Missing exercises with IDs: {MissingIds}", string.Join(", ", missingExerciseIds));
            return (null, $"The following exercise IDs do not exist: {string.Join(", ", missingExerciseIds)}");
        }


        var newRutina = new Rutina
        {
            NombreRutina = request.NombreRutina,
            Descripcion = request.Descripcion,
            Nivel = request.Nivel,
            Objetivo = request.Objetivo,
            NumeroDias = request.NumeroDias ?? request.DiasEjercicios.Select(de => de.DiaNumero).Distinct().Count(),
            IdEntrenadorCreador = creatorUserId,
            FechaCreacion = DateTime.UtcNow
        };

        foreach (var diaEjercicioRequest in request.DiasEjercicios)
        {
            newRutina.DiasEjercicios.Add(new RutinaDiaEjercicio
            {
                DiaNumero = diaEjercicioRequest.DiaNumero,
                IdEjercicio = diaEjercicioRequest.IdEjercicio,
                OrdenEnDia = diaEjercicioRequest.OrdenEnDia,
                Series = diaEjercicioRequest.Series,
                Repeticiones = diaEjercicioRequest.Repeticiones,
                DescansoSegundos = diaEjercicioRequest.DescansoSegundos,
                NotasEjercicio = diaEjercicioRequest.NotasEjercicio
            });
        }

        try
        {
            _context.Rutinas.Add(newRutina);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Routine {RoutineName} created successfully with ID {RutinaId}.", newRutina.NombreRutina, newRutina.IdRutina);

            // Reload to include exercise names in response
            var createdRutina = await GetRoutineByIdAsync(newRutina.IdRutina);
            return (createdRutina, null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating routine {RoutineName}.", request.NombreRutina);
            return (null, "Failed to create routine due to a database error. Check for unique constraints or invalid foreign keys.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error creating routine {RoutineName}.", request.NombreRutina);
            return (null, "An unexpected error occurred while creating the routine.");
        }
    }

    public async Task<RutinaResponse?> GetRoutineByIdAsync(int rutinaId)
    {
        var rutina = await _context.Rutinas
            .AsNoTracking()
            .Include(r => r.DiasEjercicios)
                .ThenInclude(de => de.Ejercicio) // Include Ejercicio to get its name
            .FirstOrDefaultAsync(r => r.IdRutina == rutinaId);

        return rutina == null ? null : MapToResponse(rutina);
    }

    public async Task<IEnumerable<RutinaResponse>> GetAllRoutinesAsync()
    {
        // For performance, might not want to include DiasEjercicios.Ejercicio here,
        // or implement pagination and selective loading.
        // For simplicity now, we include it.
        var rutinas = await _context.Rutinas
            .AsNoTracking()
            .Include(r => r.DiasEjercicios)
                .ThenInclude(de => de.Ejercicio)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();

        return rutinas.Select(MapToResponse);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateRoutineAsync(int rutinaId, UpdateRutinaRequest request, int updaterUserId)
    {
        _logger.LogInformation("Updating routine with ID {RutinaId} by User: {UpdaterId}", rutinaId, updaterUserId);

        var rutina = await _context.Rutinas
            .Include(r => r.DiasEjercicios) // Load existing day exercises to manage them
            .FirstOrDefaultAsync(r => r.IdRutina == rutinaId);

        if (rutina == null)
        {
            _logger.LogWarning("Routine with ID {RutinaId} not found for update.", rutinaId);
            return (false, "Routine not found.");
        }      

        // Validate exercise IDs exist for new/updated entries
        var exerciseIdsInRequest = request.DiasEjercicios.Select(de => de.IdEjercicio).Distinct().ToList();
        var existingExerciseIds = await _context.Ejercicios
                                            .Where(e => exerciseIdsInRequest.Contains(e.IdEjercicio))
                                            .Select(e => e.IdEjercicio)
                                            .ToListAsync();
        var missingExerciseIds = exerciseIdsInRequest.Except(existingExerciseIds).ToList();
        if (missingExerciseIds.Any())
        {
             _logger.LogWarning("Cannot update routine. Missing exercises with IDs: {MissingIds}", string.Join(", ", missingExerciseIds));
            return (false, $"The following exercise IDs do not exist: {string.Join(", ", missingExerciseIds)}");
        }


        // Update rutina properties
        rutina.NombreRutina = request.NombreRutina;
        rutina.Descripcion = request.Descripcion;
        rutina.Nivel = request.Nivel;
        rutina.Objetivo = request.Objetivo;
        rutina.NumeroDias = request.NumeroDias ?? request.DiasEjercicios.Select(de => de.DiaNumero).Distinct().Count();
        // IdEntrenadorCreador and FechaCreacion are typically not updated.

        // Simple strategy: Remove all existing day exercises and add new ones.
        // More complex strategies could involve tracking changes (add, update, delete individual day exercises).
        _context.RutinaDiaEjercicios.RemoveRange(rutina.DiasEjercicios);
        rutina.DiasEjercicios.Clear(); // Clear the collection in memory

        foreach (var diaEjercicioRequest in request.DiasEjercicios)
        {
            rutina.DiasEjercicios.Add(new RutinaDiaEjercicio
            {
                // IdRutina will be set by EF Core relationship
                DiaNumero = diaEjercicioRequest.DiaNumero,
                IdEjercicio = diaEjercicioRequest.IdEjercicio,
                OrdenEnDia = diaEjercicioRequest.OrdenEnDia,
                Series = diaEjercicioRequest.Series,
                Repeticiones = diaEjercicioRequest.Repeticiones,
                DescansoSegundos = diaEjercicioRequest.DescansoSegundos,
                NotasEjercicio = diaEjercicioRequest.NotasEjercicio
            });
        }

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Routine with ID {RutinaId} updated successfully.", rutinaId);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating routine {RutinaId}.", rutinaId);
            return (false, "Failed to update routine due to a concurrency conflict.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating routine {RutinaId}.", rutinaId);
            return (false, "Failed to update routine due to a database error.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteRoutineAsync(int rutinaId, int deleterUserId)
    {
        _logger.LogInformation("Deleting routine with ID {RutinaId} by User: {DeleterId}", rutinaId, deleterUserId);
        var rutina = await _context.Rutinas.FindAsync(rutinaId);
        if (rutina == null)
        {
            _logger.LogWarning("Routine with ID {RutinaId} not found for deletion.", rutinaId);
            return (false, "Routine not found.");
        }

        // Optional: Ownership check
        if (rutina.IdEntrenadorCreador != deleterUserId /* && !userIsAdmin */)
        {
            _logger.LogWarning("User {DeleterId} not authorized to delete routine {RutinaId} created by {CreatorId}.", deleterUserId, rutinaId, rutina.IdEntrenadorCreador);
            return (false, "You are not authorized to delete this routine.");
        }

        try
        {
            _context.Rutinas.Remove(rutina); // ON DELETE CASCADE should handle RutinaDiaEjercicios
            await _context.SaveChangesAsync();
            _logger.LogInformation("Routine with ID {RutinaId} deleted successfully.", rutinaId);
            return (true, null);
        }
        catch (DbUpdateException ex) // Handle FK constraints if routines are referenced elsewhere
        {
            _logger.LogError(ex, "Database error deleting routine {RutinaId}. It might be in use.", rutinaId);
            return (false, "Failed to delete routine. It might be assigned to users or referenced elsewhere.");
        }
    }

    private static RutinaResponse MapToResponse(Rutina rutina)
    {
        return new RutinaResponse
        {
            IdRutina = rutina.IdRutina,
            IdEntrenadorCreador = rutina.IdEntrenadorCreador,
            NombreRutina = rutina.NombreRutina,
            Descripcion = rutina.Descripcion,
            Nivel = rutina.Nivel,
            Objetivo = rutina.Objetivo,
            FechaCreacion = rutina.FechaCreacion,
            NumeroDias = rutina.NumeroDias,
            DiasEjercicios = rutina.DiasEjercicios?.Select(de => new RutinaDiaEjercicioResponse
            {
                IdRutinaDiaEjercicio = de.IdRutinaDiaEjercicio,
                DiaNumero = de.DiaNumero,
                IdEjercicio = de.IdEjercicio,
                EjercicioNombre = de.Ejercicio?.Nombre ?? "Ejercicio no encontrado", // Handle if Ejercicio not loaded
                OrdenEnDia = de.OrdenEnDia,
                Series = de.Series,
                Repeticiones = de.Repeticiones,
                DescansoSegundos = de.DescansoSegundos,
                NotasEjercicio = de.NotasEjercicio
            }).OrderBy(de => de.DiaNumero).ThenBy(de => de.OrdenEnDia).ToList() ?? new List<RutinaDiaEjercicioResponse>()
        };
    }
}