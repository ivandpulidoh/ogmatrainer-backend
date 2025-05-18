using Microsoft.EntityFrameworkCore;
using RoutineEquipmentService.Data;
using RoutineEquipmentService.Dtos;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Services;

public class EspacioService : IEspacioService
{
    private readonly RoutineEquipmentDbContext _context;
    private readonly ILogger<EspacioService> _logger;

    public EspacioService(RoutineEquipmentDbContext context, ILogger<EspacioService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(EspacioResponse? Espacio, string? ErrorMessage)> CreateEspacioAsync(CreateEspacioRequest request)
    {
        _logger.LogInformation("Creating new Espacio Deportivo: {EspacioName} for Gym ID: {GymId}",
            request.NombreEspacio, request.IdGimnasio);

        //Validar que el nombre sea unico dentro del gym
        if (await _context.EspaciosDeportivos.AnyAsync(e => e.IdGimnasio == request.IdGimnasio && e.NombreEspacio == request.NombreEspacio))
        {
            _logger.LogWarning("An Espacio Deportivo with name '{EspacioName}' already exists in Gym ID {GymId}.",
                request.NombreEspacio, request.IdGimnasio);
            return (null, $"A space with the name '{request.NombreEspacio}' already exists in this gym.");
        }

        var newEspacio = new EspacioDeportivo
        {
            IdGimnasio = request.IdGimnasio,
            NombreEspacio = request.NombreEspacio,
            Descripcion = request.Descripcion,
            Capacidad = request.Capacidad,
            Reservable = request.Reservable
        };

        try
        {
            _context.EspaciosDeportivos.Add(newEspacio);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Espacio Deportivo '{EspacioName}' (ID: {EspacioId}) created successfully.",
                newEspacio.NombreEspacio, newEspacio.IdEspacio);
            return (MapToResponse(newEspacio), null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating Espacio Deportivo '{EspacioName}'.", request.NombreEspacio);
            return (null, "Failed to create space due to a database error.");
        }
    }

    public async Task<EspacioResponse?> GetEspacioByIdAsync(int espacioId)
    {
        _logger.LogInformation("Fetching Espacio Deportivo by ID: {EspacioId} with associated machines.", espacioId);
        var espacio = await _context.EspaciosDeportivos
            .AsNoTracking()
            .Include(e => e.MaquinasEnEspacio)
            .FirstOrDefaultAsync(e => e.IdEspacio == espacioId);

        return espacio == null ? null : MapToResponse(espacio);
    }

    public async Task<IEnumerable<EspacioResponse>> GetAllEspaciosAsync(int? gymId = null)
    {        
        IQueryable<EspacioDeportivo> query = _context.EspaciosDeportivos
            .AsNoTracking()
            .Include(e => e.MaquinasEnEspacio);

        if (gymId.HasValue)
        {            
            query = query.Where(e => e.IdGimnasio == gymId.Value);
        }

        var espacios = await query.OrderBy(e => e.NombreEspacio).ToListAsync();
        return espacios.Select(MapToResponse);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateEspacioAsync(int espacioId, UpdateEspacioRequest request)
    {
        _logger.LogInformation("Updating Espacio Deportivo ID {EspacioId}", espacioId);
        var espacio = await _context.EspaciosDeportivos.FindAsync(espacioId);

        if (espacio == null)
        {
            _logger.LogWarning("Espacio Deportivo ID {EspacioId} not found for update.", espacioId);
            return (false, "Space not found.");
        }
        
        if (!string.IsNullOrEmpty(request.NombreEspacio) && request.NombreEspacio != espacio.NombreEspacio)
        {
            if (await _context.EspaciosDeportivos.AnyAsync(e => e.IdEspacio != espacioId &&
                                                               e.IdGimnasio == espacio.IdGimnasio &&
                                                               e.NombreEspacio == request.NombreEspacio))
            {
                _logger.LogWarning("Cannot update Espacio. Another space with name '{NewName}' already exists in Gym ID {GymId}.",
                    request.NombreEspacio, espacio.IdGimnasio);
                return (false, $"A space with the name '{request.NombreEspacio}' already exists in this gym.");
            }
            espacio.NombreEspacio = request.NombreEspacio;
        }

        if (request.Descripcion != null) espacio.Descripcion = request.Descripcion;
        if (request.Capacidad.HasValue) espacio.Capacidad = request.Capacidad.Value;
        if (request.Reservable.HasValue) espacio.Reservable = request.Reservable.Value;        

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Espacio Deportivo ID {EspacioId} updated successfully.", espacioId);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating Espacio Deportivo ID {EspacioId}.", espacioId);
            return (false, "Failed to update space due to a concurrency conflict.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating Espacio Deportivo ID {EspacioId}.", espacioId);
            return (false, "Failed to update space due to a database error.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteEspacioAsync(int espacioId)
    {
        _logger.LogInformation("Deleting Espacio Deportivo ID {EspacioId}", espacioId);
        var espacio = await _context.EspaciosDeportivos.FindAsync(espacioId);

        if (espacio == null)
        {
            _logger.LogWarning("Espacio Deportivo ID {EspacioId} not found for deletion.", espacioId);
            return (false, "Space not found.");
        }

        // Revisar si hay una dependencia
        bool isSpaceInUseByMachines = await _context.MaquinasEjercicio.AnyAsync(m => m.IdEspacio == espacioId);
        if (isSpaceInUseByMachines)
        {
            _logger.LogWarning("Cannot delete Espacio Deportivo ID {EspacioId} as it has machines associated with it.", espacioId);
            return (false, "Cannot delete space. It has equipment/machines assigned to it. Please reassign or delete the equipment first.");
        }        

        try
        {
            _context.EspaciosDeportivos.Remove(espacio);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Espacio Deportivo ID {EspacioId} deleted successfully.", espacioId);
            return (true, null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting Espacio Deportivo ID {EspacioId}.", espacioId);        
            return (false, "Failed to delete space. It might still be in use.");
        }
    }

    private EspacioResponse MapToResponse(EspacioDeportivo espacio)
    {
        return new EspacioResponse
        {
            IdEspacio = espacio.IdEspacio,
            IdGimnasio = espacio.IdGimnasio,
            NombreEspacio = espacio.NombreEspacio,
            Descripcion = espacio.Descripcion,
            Capacidad = espacio.Capacidad,
            Reservable = espacio.Reservable,
            MaquinasEnEspacio = espacio.MaquinasEnEspacio?
                .Select(m => new SimpleMaquinaDto
                {
                    IdMaquina = m.IdMaquina,
                    Nombre = m.Nombre,
                    TipoMaquina = m.TipoMaquina,
                    UrlImagen = m.UrlImagen,
                    Estado = m.Estado,
                    Reservable = m.Reservable
                }).ToList() ?? new List<SimpleMaquinaDto>()
        };
    }  
}