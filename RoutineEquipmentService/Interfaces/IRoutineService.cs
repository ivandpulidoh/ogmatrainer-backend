using RoutineEquipmentService.Dtos;
using RoutineEquipmentService.Models;
namespace RoutineEquipmentService.Interfaces;

public interface IRoutineService
{
    Task<(RutinaResponse? Routine, string? ErrorMessage)> CreateRoutineAsync(CreateRutinaRequest request, int creatorUserId);
    Task<RutinaResponse?> GetRoutineByIdAsync(int rutinaId);
    Task<IEnumerable<RutinaResponse>> GetAllRoutinesAsync(); // Or paged
    Task<(bool Success, string? ErrorMessage)> UpdateRoutineAsync(int rutinaId, UpdateRutinaRequest request, int updaterUserId);
    Task<(bool Success, string? ErrorMessage)> DeleteRoutineAsync(int rutinaId, int deleterUserId);
    Task<(IEnumerable<MaquinaResponse>? Maquinas, string? ErrorMessage)> GetMaquinasForRutinaAsync(int rutinaId);
    Task<RutinaDiaEjercicioResponse?> GetRutinaDiaEjercicioByIdAsync(int idRutinaDiaEjercicio);
    Task<(AssignedRutinaResponse? AssignedRutina, string? ErrorMessage)> AssignRutinaToUserAsync(AssignRutinaRequest request, int idEntrenadorAsignador);
    Task<IEnumerable<AssignedRutinaResponse>> GetRutinasForUserAsync(int idUsuario);
    Task<IEnumerable<AssignedRutinaResponse>> GetRutinasAssignedByTrainerAsync(int idUsuario, int idEntrenadorAsignador);
    Task<(bool Success, string? ErrorMessage)> SetRutinaActiveStateAsync(int idUsuarioRutina, bool activa, int requestingUserId);
}