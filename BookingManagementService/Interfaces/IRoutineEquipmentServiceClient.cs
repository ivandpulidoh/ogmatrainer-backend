using BookingManagementService.Models.External;
using System.Threading.Tasks;

namespace BookingManagementService.Interfaces;

public interface IRoutineEquipmentServiceClient
{
    Task<ExternalRoutineDto?> GetRoutineByIdAsync(int routineId);
    Task<ExternalExerciseDto?> GetExerciseByIdAsync(int exerciseId);
    Task<ExternalRutinaDiaEjercicioDto?> GetRutinaDiaEjercicioByIdAsync(int idRutinaDiaEjercicio);
}