using CapacityControlService.Dtos;
namespace CapacityControlService.Interfaces;
public interface IAttendanceService {
    Task<(CheckInResponse? Response, string? ErrorMessage, bool CapacityReached)> CheckInAsync(CheckInRequest request);
    Task<(bool Success, string? ErrorMessage)> CheckOutAsync(CheckOutRequest request);
}