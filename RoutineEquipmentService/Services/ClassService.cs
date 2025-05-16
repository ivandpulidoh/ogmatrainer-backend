using Microsoft.EntityFrameworkCore;
using RoutineEquipmentService.Data;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Services;

public class ClassService : IClassService
{
    private readonly RoutineEquipmentDbContext _context;
    private readonly ILogger<ClassService> _logger;

    public ClassService(RoutineEquipmentDbContext context, ILogger<ClassService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(ClaseResponse? Clase, string? ErrorMessage)> CreateClaseAsync(CreateClaseRequest request, int creatorUserId)
    {
        _logger.LogInformation("Creating new class '{ClassName}' by User {UserId} for Gym {GymId}",
            request.NombreClase, creatorUserId, request.IdGimnasio);

        // Basic validation (DTO should handle most)
        if (request.Tipo == "EnVivo" && (!request.FechaHoraInicio.HasValue || !request.DuracionMinutos.HasValue))
        {
            return (null, "For 'EnVivo' classes, FechaHoraInicio and DuracionMinutos are required.");
        }
        if (request.Tipo == "Grabada" && string.IsNullOrEmpty(request.UrlClase))
        {
            return (null, "For 'Grabada' classes, UrlClase is required.");
        }
        

        var newClase = new Clase
        {
            IdGimnasio = request.IdGimnasio,
            IdEntrenador = request.IdEntrenador,
            NombreClase = request.NombreClase,
            Descripcion = request.Descripcion,
            Tipo = request.Tipo,
            UrlClase = request.UrlClase,
            UrlImagen = request.UrlImagen,
            FechaHoraInicio = request.Tipo == "EnVivo" ? request.FechaHoraInicio?.ToUniversalTime() : null,
            DuracionMinutos = request.Tipo == "EnVivo" ? request.DuracionMinutos : null,
            CapacidadMaxima = request.Tipo == "EnVivo" ? request.CapacidadMaxima : null,
            Activa = request.Activa            
        };

        try
        {
            _context.Clases.Add(newClase);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Class '{ClassName}' (ID: {ClassId}) created successfully.", newClase.NombreClase, newClase.IdClase);
            return (MapToResponse(newClase), null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating class '{ClassName}'.", request.NombreClase);
            // Check for specific constraint violations (e.g., unique name within a gym?)
            return (null, "Failed to create class due to a database error.");
        }
    }

    public async Task<ClaseResponse?> GetClaseByIdAsync(int claseId)
    {
        var clase = await _context.Clases
            .AsNoTracking()            
            .FirstOrDefaultAsync(c => c.IdClase == claseId);

        return clase == null ? null : MapToResponse(clase);
    }

    public async Task<IEnumerable<ClaseResponse>> GetAllClasesAsync(int? gymId = null, string? tipo = null, bool? activa = null)
    {
        var query = _context.Clases.AsNoTracking();

        if (gymId.HasValue)
        {
            query = query.Where(c => c.IdGimnasio == gymId.Value);
        }
        if (!string.IsNullOrEmpty(tipo))
        {
            query = query.Where(c => c.Tipo == tipo);
        }
        if (activa.HasValue)
        {
            query = query.Where(c => c.Activa == activa.Value);
        }

        var clases = await query.OrderByDescending(c => c.FechaHoraInicio ?? DateTime.MinValue)
                                 .ThenBy(c => c.NombreClase)
                                 .ToListAsync();
        return clases.Select(MapToResponse);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateClaseAsync(int claseId, UpdateClaseRequest request, int updaterUserId)
    {
        _logger.LogInformation("Updating class ID {ClassId} by User {UserId}", claseId, updaterUserId);
        var clase = await _context.Clases.FindAsync(claseId);

        if (clase == null)
        {
            _logger.LogWarning("Class ID {ClassId} not found for update.", claseId);
            return (false, "Class not found.");
        }        

        // Update fields if provided in the request
        if (request.IdGimnasio.HasValue) clase.IdGimnasio = request.IdGimnasio.Value;
        // Handle clearing IdEntrenador if explicitly set to null or a sentinel value
        if (request.IdEntrenador.HasValue || request.GetType().GetProperty(nameof(request.IdEntrenador))?.GetValue(request, null) == null)
        {
             clase.IdEntrenador = request.IdEntrenador;
        }
        if (!string.IsNullOrEmpty(request.NombreClase)) clase.NombreClase = request.NombreClase;
        if (request.Descripcion != null) clase.Descripcion = request.Descripcion; // Allow setting to "" or null
        if (!string.IsNullOrEmpty(request.Tipo)) clase.Tipo = request.Tipo;
        if (request.UrlClase != null) clase.UrlClase = request.UrlClase;
        if (request.UrlImagen != null) clase.UrlImagen = request.UrlImagen; 
        if (request.FechaHoraInicio.HasValue) clase.FechaHoraInicio = request.FechaHoraInicio.Value.ToUniversalTime();
        if (request.DuracionMinutos.HasValue) clase.DuracionMinutos = request.DuracionMinutos.Value;
        if (request.CapacidadMaxima.HasValue) clase.CapacidadMaxima = request.CapacidadMaxima.Value;
        if (request.Activa.HasValue) clase.Activa = request.Activa.Value;

        // Re-validate if Tipo changed crucial fields
        if (clase.Tipo == "EnVivo" && (!clase.FechaHoraInicio.HasValue || !clase.DuracionMinutos.HasValue))
        {
            return (false, "For 'EnVivo' classes, FechaHoraInicio and DuracionMinutos are required and must be valid.");
        }
         if (clase.Tipo == "Grabada" && string.IsNullOrEmpty(clase.UrlClase))
        {
            return (false, "For 'Grabada' classes, UrlClase is required.");
        }


        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Class ID {ClassId} updated successfully.", claseId);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating class ID {ClassId}.", claseId);
            return (false, "Failed to update class due to a concurrency conflict.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating class ID {ClassId}.", claseId);
            return (false, "Failed to update class due to a database error.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteClaseAsync(int claseId, int deleterUserId)
    {
        _logger.LogInformation("Deleting class ID {ClassId} by User {UserId}", claseId, deleterUserId);
        var clase = await _context.Clases.FindAsync(claseId);

        if (clase == null)
        {
            _logger.LogWarning("Class ID {ClassId} not found for deletion.", claseId);
            return (false, "Class not found.");
        }
       
        try
        {
            _context.Clases.Remove(clase);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Class ID {ClassId} deleted successfully.", claseId);
            return (true, null);
        }
        catch (DbUpdateException ex) // Handle FK constraints (e.g., if class has active registrations)
        {
            _logger.LogError(ex, "Database error deleting class ID {ClassId}. It might be in use.", claseId);
            return (false, "Failed to delete class. It might have active registrations or be referenced elsewhere.");
        }
    }

    private ClaseResponse MapToResponse(Clase clase)
    {
        return new ClaseResponse
        {
            IdClase = clase.IdClase,
            IdGimnasio = clase.IdGimnasio,
            IdEntrenador = clase.IdEntrenador,            
            NombreClase = clase.NombreClase,
            Descripcion = clase.Descripcion,
            Tipo = clase.Tipo,
            UrlClase = clase.UrlClase,
            UrlImagen = clase.UrlImagen,
            FechaHoraInicio = clase.FechaHoraInicio,
            DuracionMinutos = clase.DuracionMinutos,
            CapacidadMaxima = clase.CapacidadMaxima,
            Activa = clase.Activa
        };
    }
}