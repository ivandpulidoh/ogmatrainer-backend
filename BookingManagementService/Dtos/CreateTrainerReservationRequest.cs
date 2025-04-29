using System.ComponentModel.DataAnnotations;

namespace BookingManagementService.Models;

public class CreateTrainerReservationRequest
{
    [Required]
    public int IdCliente { get; set; }
    [Required]
    public int IdEntrenador { get; set; }
    public int? IdEspacio { get; set; } // Optional space
    [Required]
    public DateTime FechaHoraInicio { get; set; }
    [Required]
    public DateTime FechaHoraFin { get; set; }

    public bool IsValid() => IdCliente > 0 && IdEntrenador > 0 && FechaHoraFin > FechaHoraInicio;
}