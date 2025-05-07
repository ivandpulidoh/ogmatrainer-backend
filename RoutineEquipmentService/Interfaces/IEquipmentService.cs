using RoutineEquipmentService.Dtos;
namespace RoutineEquipmentService.Interfaces;
public interface IEquipmentService {
    Task<(MaquinaResponse? Machine, string? ErrorMessage)> CreateMachineAsync(CreateMaquinaRequest request);
    Task<MaquinaResponse?> GetMachineByIdAsync(int machineId);
    Task<(bool Success, string? ErrorMessage)> UpdateMachineAsync(int machineId, UpdateMaquinaRequest request);
    Task<(bool Success, string? ErrorMessage)> DeleteMachineAsync(int machineId);
    Task<IEnumerable<MaquinaResponse>> GetAllMachinesAsync();
}