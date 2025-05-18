using System.ComponentModel.DataAnnotations;

namespace BookingManagementService.Models;

public class ValidateRoutineDayAvailabilityRequest
{
    [Required]
    public int RoutineId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DiaNumero must be a positive integer.")]
    public int DiaNumero { get; set; }
    [Required]
    public DateTime DesiredStartDateTime { get; set; }    
}