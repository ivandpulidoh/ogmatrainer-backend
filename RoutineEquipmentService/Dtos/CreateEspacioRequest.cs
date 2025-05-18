using System.ComponentModel.DataAnnotations;
namespace RoutineEquipmentService.Dtos;

public class CreateEspacioRequest
{
    [Required]
    public int IdGimnasio { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string NombreEspacio { get; set; } = null!;

    public string? Descripcion { get; set; }
    
    public int Capacidad { get; set; } = 1;

    public bool Reservable { get; set; } = true;
}