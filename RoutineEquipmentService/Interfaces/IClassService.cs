using RoutineEquipmentService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Interfaces;

public interface IClassService
{
    Task<(ClaseResponse? Clase, string? ErrorMessage)> CreateClaseAsync(CreateClaseRequest request, int creatorUserId);
    Task<ClaseResponse?> GetClaseByIdAsync(int claseId);
    Task<IEnumerable<ClaseResponse>> GetAllClasesAsync(int? gymId = null, string? tipo = null, bool? activa = null);
    Task<(bool Success, string? ErrorMessage)> UpdateClaseAsync(int claseId, UpdateClaseRequest request, int updaterUserId);
    Task<(bool Success, string? ErrorMessage)> DeleteClaseAsync(int claseId, int deleterUserId);
}