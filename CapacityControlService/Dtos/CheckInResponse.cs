namespace CapacityControlService.Dtos;
public class CheckInResponse
{
    public int CheckInId { get; set; }
    public int UserId { get; set; }
    public int GymId { get; set; }
    public DateTime EntryTime { get; set; }
    public bool RequiresSymptomForm { get; set; } // Indicate if form needed
}