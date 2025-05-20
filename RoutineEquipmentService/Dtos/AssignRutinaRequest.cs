using System.ComponentModel.DataAnnotations;

namespace RoutineEquipmentService.Models;

public class AssignRutinaRequest
{
    [Required(ErrorMessage = "El ID del usuario es requerido.")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El ID de la rutina es requerido.")]
    public int IdRutina { get; set; }
    
    public int IdEntrenadorAsignador { get; set; }
}