using CapacityControlService.Dtos;
namespace CapacityControlService.Interfaces;
public interface ISymptomService {
    Task<(SymptomFormResponse? Response, string? ErrorMessage)> SubmitFormAsync(SymptomFormRequest request);
    Task<SymptomFormResponse?> GetFormByIdAsync(int formId);
    Task<SymptomFormResponse?> GetFormByCheckInIdAsync(int checkInId);
}