using RoutineEquipmentService.Dtos;

namespace RoutineEquipmentService.Models;

public class AssignedRutinaResponse
{
    public int IdUsuarioRutina { get; set; }
    public int IdUsuario { get; set; }
    public int IdEntrenadorAsignador { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public bool Activa { get; set; }
    public RutinaResponse? RutinaDetalles { get; set; } 
}