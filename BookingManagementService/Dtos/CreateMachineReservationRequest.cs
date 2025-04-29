using System.ComponentModel.DataAnnotations;

namespace BookingManagementService.Models;

public class CreateMachineReservationRequest
{
    [Required]
    public int IdUsuario { get; set; }
    [Required]
    public int IdMaquina { get; set; }
    [Required]
    public DateTime FechaHoraInicio { get; set; }
    [Required]
    public DateTime FechaHoraFin { get; set; }

    // Basic validation
    public bool IsValid() => IdUsuario > 0 && IdMaquina > 0 && FechaHoraFin > FechaHoraInicio;
}