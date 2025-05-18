namespace BookingManagementService.Models;

public class RoutineDayBookingSummary
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = null!;
    public int MachineId { get; set; }
    public string MachineName { get; set; } = null!;
    public int ReservationId { get; set; }
    public DateTime ReservedStartTime { get; set; }
    public DateTime ReservedEndTime { get; set; }
}

public class RoutineDayBookingResponse
{
    public int UserId { get; set; }
    public int RoutineId { get; set; }
    public int DiaNumero { get; set; }
    public string Message { get; set; } = null!;
    public List<RoutineDayBookingSummary> BookedMachines { get; set; } = new();
}