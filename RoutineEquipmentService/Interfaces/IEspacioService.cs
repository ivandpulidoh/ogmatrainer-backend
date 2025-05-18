using RoutineEquipmentService.Dtos;
using RoutineEquipmentService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Interfaces;

public interface IEspacioService
{
    Task<(EspacioResponse? Espacio, string? ErrorMessage)> CreateEspacioAsync(CreateEspacioRequest request);
    Task<EspacioResponse?> GetEspacioByIdAsync(int espacioId);
    Task<IEnumerable<EspacioResponse>> GetAllEspaciosAsync(int? gymId = null);
    Task<(bool Success, string? ErrorMessage)> UpdateEspacioAsync(int espacioId, UpdateEspacioRequest request);
    Task<(bool Success, string? ErrorMessage)> DeleteEspacioAsync(int espacioId);
}