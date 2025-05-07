using System.ComponentModel.DataAnnotations;
namespace CapacityControlService.Dtos;
public class SymptomFormRequest
{
    [Required]
    public int CheckInId { get; set; } // Link to the specific check-in
    [Required]
    public int UserId { get; set; }
    [Required]
    public bool HasSymptoms { get; set; }
    [Required]
    public bool HasRecentContact { get; set; }
}