using RoutineEquipmentService.Dtos;
namespace RoutineEquipmentService.Interfaces;

public interface IRoutineService
{
    Task<(RutinaResponse? Routine, string? ErrorMessage)> CreateRoutineAsync(CreateRutinaRequest request, int creatorUserId);
    Task<RutinaResponse?> GetRoutineByIdAsync(int rutinaId);
    Task<IEnumerable<RutinaResponse>> GetAllRoutinesAsync(); // Or paged
    Task<(bool Success, string? ErrorMessage)> UpdateRoutineAsync(int rutinaId, UpdateRutinaRequest request, int updaterUserId);
    Task<(bool Success, string? ErrorMessage)> DeleteRoutineAsync(int rutinaId, int deleterUserId);
    Task<(IEnumerable<MaquinaResponse>? Maquinas, string? ErrorMessage)> GetMaquinasForRutinaAsync(int rutinaId);
}