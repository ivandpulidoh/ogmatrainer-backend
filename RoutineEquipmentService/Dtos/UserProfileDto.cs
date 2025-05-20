using System.ComponentModel.DataAnnotations;

namespace RoutineEquipmentService.Models;

public class UserProfileDto
{
    public double? AlturaCm { get; set; }
    public double? PesoInicialKg { get; set; }
    [Required(ErrorMessage = "El peso actual es requerido.")]
    public double PesoActualKg { get; set; }
    public double? PesoObjetivoKg { get; set; }

    [Required(ErrorMessage = "El objetivo principal es requerido.")]
    [StringLength(500)]
    public string ObjetivoPrincipal { get; set; } = null!;

    [Required(ErrorMessage = "La experiencia de entrenamiento es requerida.")]
    [StringLength(50)]
    public string ExperienciaEntrenamiento { get; set; } = null!; // Principiante, Intermedio, Avanzado, Ninguna

    [StringLength(50)]
    public string? NivelActividadDiaria { get; set; } // Sedentario, Ligero, Moderado, Activo, MuyActivo

    public string? CondicionesMedicas { get; set; }

    [Required(ErrorMessage = "La disponibilidad de entrenamiento es requerida.")]
    public string DisponibilidadEntrenamiento { get; set; } = null!;

    [StringLength(50)]
    public string? PreferenciaLugarEntrenamiento { get; set; } // Casa, AireLibre, Gimnasio, Mixto, Indiferente
}