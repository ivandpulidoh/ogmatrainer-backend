using CapacityControlService.Dtos;
namespace CapacityControlService.Interfaces;
public interface ICapacityService {
    Task CheckCapacityAndNotifyAsync(int gymId, int currentOccupancy);
    Task<IEnumerable<HistoricalCapacityPoint>> GetHistoricalCapacityAsync(int gymId, DateTime startDate, DateTime endDate);
}