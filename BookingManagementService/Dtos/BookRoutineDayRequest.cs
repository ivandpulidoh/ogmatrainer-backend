using System.ComponentModel.DataAnnotations;

namespace BookingManagementService.Models;

public class BookRoutineDayRequest
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public int RoutineId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DiaNumero must be a positive integer.")]
    public int DiaNumero { get; set; } // Which day of the routine to book
    [Required]
    public DateTime StartDateTime { get; set; } // The desired start time for the FIRST exercise of this routine day
}