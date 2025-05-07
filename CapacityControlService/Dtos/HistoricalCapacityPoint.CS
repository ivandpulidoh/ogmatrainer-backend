namespace CapacityControlService.Dtos;
public class HistoricalCapacityPoint
{
    public DateTime Timestamp { get; set; } // Represents start of the interval (e.g., hour)
    public int Occupancy { get; set; }
}