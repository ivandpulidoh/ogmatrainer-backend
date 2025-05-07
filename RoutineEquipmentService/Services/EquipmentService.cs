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

public class EquipmentService : IEquipmentService
{
    private readonly RoutineEquipmentDbContext _context;
    private readonly IExternalQrCodeService _qrCodeService;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(RoutineEquipmentDbContext context, IExternalQrCodeService qrCodeService, ILogger<EquipmentService> logger)
    {
        _context = context;
        _qrCodeService = qrCodeService;
        _logger = logger;
    }

    public async Task<(MaquinaResponse? Machine, string? ErrorMessage)> CreateMachineAsync(CreateMaquinaRequest request)
    {
        _logger.LogInformation("Creating new machine: {MachineName}", request.Nombre);

        // Validate Espacio exists
        var espacioExists = await _context.EspaciosDeportivos.AnyAsync(e => e.IdEspacio == request.IdEspacio);
        if (!espacioExists)
        {
            _logger.LogWarning("Espacio with ID {EspacioId} not found for new machine.", request.IdEspacio);
            return (null, "Specified space (IdEspacio) not found.");
        }

        // Call external QR Code service
        byte[]? qrCodeBytes = await _qrCodeService.GetQrCodeBytesAsync(request.Nombre, request.Descripcion);
        if (qrCodeBytes == null)
        {
            _logger.LogWarning("Failed to generate QR code for machine {MachineName}. Machine will be created without a QR code.", request.Nombre);
            return (null, "Failed to generate QR code for machine (MachineName)");
        }

        var newMachine = new MaquinaEjercicio
        {
            IdEspacio = request.IdEspacio,
            Nombre = request.Nombre,
            TipoMaquina = request.TipoMaquina,
            Descripcion = request.Descripcion,
            FechaAdquisicion = request.FechaAdquisicion,
            Estado = request.Estado,
            Reservable = request.Reservable,
            CodigoQr = qrCodeBytes 
        };

        try
        {
            _context.MaquinasEjercicio.Add(newMachine);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Machine {MachineName} created successfully with ID {MachineId}.", newMachine.Nombre, newMachine.IdMaquina);

            return (MapToResponseStatic(newMachine), null);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating machine {MachineName}.", request.Nombre);            
            return (null, "Failed to create machine due to a database error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error creating machine {MachineName}.", request.Nombre);
            return (null, "An unexpected error occurred.");
        }
    }

    public async Task<MaquinaResponse?> GetMachineByIdAsync(int machineId)
    {
        var machine = await _context.MaquinasEjercicio.AsNoTracking().FirstOrDefaultAsync(m => m.IdMaquina == machineId);
        return machine == null ? null : MapToResponseStatic(machine);
    }

    public async Task<IEnumerable<MaquinaResponse>> GetAllMachinesAsync()
    {
        return await _context.MaquinasEjercicio
            .AsNoTracking()
            .Select(m => MapToResponseStatic(m))
            .ToListAsync();
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateMachineAsync(int machineId, UpdateMaquinaRequest request)
    {
         _logger.LogInformation("Updating machine with ID {MachineId}", machineId);
        var machine = await _context.MaquinasEjercicio.FindAsync(machineId);
        if (machine == null)
        {
            _logger.LogWarning("Machine with ID {MachineId} not found for update.", machineId);
            return (false, "Machine not found.");
        }

        // Validate new Espacio exists if it's being changed
        if (request.IdEspacio.HasValue && request.IdEspacio.Value != machine.IdEspacio)
        {
            var newEspacioExists = await _context.EspaciosDeportivos.AnyAsync(e => e.IdEspacio == request.IdEspacio.Value);
            if (!newEspacioExists)
            {
                 _logger.LogWarning("New Espacio with ID {EspacioId} not found for machine update.", request.IdEspacio.Value);
                return (false, "New specified space (IdEspacio) not found.");
            }
            machine.IdEspacio = request.IdEspacio.Value;
        }

        // Update properties if provided
        if (!string.IsNullOrEmpty(request.Nombre)) machine.Nombre = request.Nombre;
        if (!string.IsNullOrEmpty(request.TipoMaquina)) machine.TipoMaquina = request.TipoMaquina;
        if (request.Descripcion != null) machine.Descripcion = request.Descripcion; // Allow setting to empty string
        if (request.FechaAdquisicion.HasValue) machine.FechaAdquisicion = request.FechaAdquisicion.Value;
        if (!string.IsNullOrEmpty(request.Estado)) machine.Estado = request.Estado; // TODO: Validate estado value
        if (request.Reservable.HasValue) machine.Reservable = request.Reservable.Value;

        // If name or description changed, regenerate QR code
         if ((!string.IsNullOrEmpty(request.Nombre) && request.Nombre != machine.Nombre) ||
             (request.Descripcion != null && request.Descripcion != machine.Descripcion))
         {
             _logger.LogInformation("Machine name or description changed for ID {MachineId}. Regenerating QR code.", machineId);
             byte[]? qrCodeBytes = await _qrCodeService.GetQrCodeBytesAsync(machine.Nombre, machine.Descripcion);
             if (qrCodeBytes != null)
             {
                 machine.CodigoQr = qrCodeBytes;
             } else {
                 _logger.LogWarning("Failed to regenerate QR code for machine {MachineName}. Existing QR will be kept if any.", machine.Nombre);
             }
         }

        try
        {
            await _context.SaveChangesAsync();
             _logger.LogInformation("Machine with ID {MachineId} updated successfully.", machineId);
            return (true, null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
             _logger.LogError(ex, "Concurrency error updating machine {MachineId}.", machineId);
            return (false, "Failed to update machine due to a concurrency conflict.");
        }
        catch (DbUpdateException ex)
        {
             _logger.LogError(ex, "Database error updating machine {MachineId}.", machineId);
            return (false, "Failed to update machine due to a database error.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteMachineAsync(int machineId)
    {
        _logger.LogInformation("Deleting machine with ID {MachineId}", machineId);
        var machine = await _context.MaquinasEjercicio.FindAsync(machineId);
        if (machine == null)
        {
            _logger.LogWarning("Machine with ID {MachineId} not found for deletion.", machineId);
            return (false, "Machine not found.");
        }

        try
        {
            _context.MaquinasEjercicio.Remove(machine);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Machine with ID {MachineId} deleted successfully.", machineId);
            return (true, null);
        }
        catch (DbUpdateException ex) // Handle FK constraints if machines are referenced elsewhere (e.g., reservations)
        {
             _logger.LogError(ex, "Database error deleting machine {MachineId}. It might be in use.", machineId);
            return (false, "Failed to delete machine. It might be referenced by other records (e.g., reservations).");
        }
    }    

    private static MaquinaResponse MapToResponseStatic(MaquinaEjercicio machine)
    {
        return new MaquinaResponse
        {
            IdMaquina = machine.IdMaquina,
            IdEspacio = machine.IdEspacio,
            Nombre = machine.Nombre,
            TipoMaquina = machine.TipoMaquina,
            Descripcion = machine.Descripcion,
            FechaAdquisicion = machine.FechaAdquisicion,
            Estado = machine.Estado,
            Reservable = machine.Reservable,
            CodigoQrBase64 = machine.CodigoQr != null ? Convert.ToBase64String(machine.CodigoQr) : null
        };
    }
}