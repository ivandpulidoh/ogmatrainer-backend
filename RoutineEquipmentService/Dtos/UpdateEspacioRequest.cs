using System.ComponentModel.DataAnnotations;
namespace RoutineEquipmentService.Models;

public class UpdateEspacioRequest
{
    
    [StringLength(100, MinimumLength = 3)]
    public string? NombreEspacio { get; set; }

    public string? Descripcion { get; set; }

    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
    public int? Capacidad { get; set; }

    public bool? Reservable { get; set; }
}