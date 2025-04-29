namespace BookingManagementService.Models;

public class BookingDto
{
    public int ReservationId { get; set; }
    public string ReservationType { get; set; } = null!; // e.g., "Machine", "Trainer", "Class"
    public int UserId { get; set; } // Could be ClientId for trainer booking
    public string? ItemName { get; set; } // Machine name, Trainer name, Class name
    public int? ItemId { get; set; } // MachineId, TrainerId, ClassId
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
    public bool? Attended { get; set; }
}