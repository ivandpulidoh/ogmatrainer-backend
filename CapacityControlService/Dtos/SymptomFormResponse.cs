namespace CapacityControlService.Dtos;
public class SymptomFormResponse
{
    public int FormId { get; set; }
    public int CheckInId { get; set; }
    public int UserId { get; set; }
    public DateTime SubmissionTime { get; set; }
    public bool HasSymptoms { get; set; }
    public bool HasRecentContact { get; set; }
    public string EvaluationResult { get; set; } = null!; // Aprobado, Rechazado
}