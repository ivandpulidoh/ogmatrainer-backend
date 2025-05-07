using RoutineEquipmentService.Dtos;
namespace RoutineEquipmentService.Interfaces;
public interface IExerciseService {
    Task<(EjercicioResponse? Exercise, string? ErrorMessage)> CreateExerciseAsync(CreateEjercicioRequest request, int? creatorUserId);
    Task<EjercicioResponse?> GetExerciseByIdAsync(int exerciseId);
    Task<IEnumerable<EjercicioResponse>> GetAllExercisesAsync();
    Task<(bool Success, string? ErrorMessage)> UpdateExerciseAsync(int exerciseId, UpdateEjercicioRequest request, int? updaterUserId);
    Task<(bool Success, string? ErrorMessage)> DeleteExerciseAsync(int exerciseId, int? deleterUserId);
}