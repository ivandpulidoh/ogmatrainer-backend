using RoutineEquipmentService.Dtos;
using RoutineEquipmentService.Models;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Interfaces;

public interface IAiRoutineGeneratorService
{
    Task<(CreateRutinaRequest? GeneratedRoutine, string? ErrorMessage)> GenerateRoutineAsync(UserProfileDto userProfile, int creatorUserId);
}