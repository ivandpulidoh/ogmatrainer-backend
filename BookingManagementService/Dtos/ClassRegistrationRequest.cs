using System.ComponentModel.DataAnnotations;

namespace BookingManagementService.Models;

public class ClassRegistrationRequest
{
    [Required]
    public int IdUsuario { get; set; }
}