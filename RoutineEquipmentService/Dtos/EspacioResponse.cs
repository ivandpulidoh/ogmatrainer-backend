using RoutineEquipmentService.Dtos;

namespace RoutineEquipmentService.Models;

public class EspacioResponse
{
    public int IdEspacio { get; set; }
    public int IdGimnasio { get; set; }
    public string NombreEspacio { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int Capacidad { get; set; }
    public bool Reservable { get; set; }

    public List<SimpleMaquinaDto> MaquinasEnEspacio { get; set; } = new List<SimpleMaquinaDto>();
}