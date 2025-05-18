public class ExerciseMachineAvailability
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = null!;
    public bool IsAvailable { get; set; }
    public int? AvailableMachineId { get; set; }
    public string? AvailableMachineName { get; set; }
    public DateTime? SuggestedStartTime { get; set; } // If available
    public string? ReasonIfNotAvailable { get; set; }
}
public class RoutineDayAvailabilityResponse
{
    public bool IsOverallAvailable { get; set; }
    public string Message { get; set; } = null!;
    public DateTime OriginalRequestedStartTime { get; set; }
    public DateTime? ActualPossibleStartTime { get; set; } // If shifts are needed
    public List<ExerciseMachineAvailability> ExerciseAvailabilities { get; set; } = new();
}