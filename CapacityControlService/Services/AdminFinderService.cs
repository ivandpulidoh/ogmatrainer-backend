using CapacityControlService.Data;
using CapacityControlService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapacityControlService.Services;

public class AdminFinderService : IAdminFinderService
{
    private readonly CapacityDbContext _context;
    private readonly ILogger<AdminFinderService> _logger;
    // Define admin role names consistently
    private readonly string[] _adminRoleNames = { "Administrador", "AdminGimnasio" };


    public AdminFinderService(CapacityDbContext context, ILogger<AdminFinderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<int>> GetAdminIdsForGymAsync(int gymId)
    {
        try
        {
            var gymSpecificAdminIds = await _context.GimnasioAdministradores
                                                .Where(ga => ga.IdGimnasio == gymId)
                                                .Select(ga => ga.IdUsuario)
                                                .ToListAsync();

            var globalAdminIds = await _context.UsuarioRoles
                                           .Where(ur => ur.Rol != null && _adminRoleNames.Contains(ur.Rol.NombreRol))
                                           .Select(ur => ur.IdUsuario)
                                           .ToListAsync();

            var allAdminIds = gymSpecificAdminIds.Union(globalAdminIds).Distinct();

            _logger.LogInformation("Found {Count} administrators for Gym {GymId}.", allAdminIds.Count(), gymId);
            return allAdminIds;
        }
        catch(Exception ex)
        {
             _logger.LogError(ex, "Error finding administrators for Gym {GymId}", gymId);
             return Enumerable.Empty<int>(); // Return empty list on error
        }
    }
}