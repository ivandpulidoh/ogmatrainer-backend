using System.ComponentModel.DataAnnotations;
namespace RoutineEquipmentService.Dtos;

public class CreateRutinaRequest
{
    [Required]
    [MaxLength(150)]
    public string NombreRutina { get; set; } = null!;
    public string? Descripcion { get; set; }
    [MaxLength(15)]
    public string? Nivel { get; set; } // Principiante, Intermedio, Avanzado
    [MaxLength(100)]
    public string? Objetivo { get; set; }
    public int? NumeroDias { get; set; }
    public string? UrlImagen { get; set; }
    public List<RutinaDiaEjercicioRequest> DiasEjercicios { get; set; } = new();
}

public class RutinaDiaEjercicioRequest
{
    [Required]
    public int DiaNumero { get; set; }
    [Required]
    public int IdEjercicio { get; set; }
    [Required]
    public int OrdenEnDia { get; set; }
    [MaxLength(20)]
    public string? Series { get; set; }
    [MaxLength(20)]
    public string? Repeticiones { get; set; }
    public int? DescansoSegundos { get; set; }
    public string? NotasEjercicio { get; set; }
}

public class UpdateRutinaRequest : CreateRutinaRequest {}

public class RutinaResponse
{
    public int IdRutina { get; set; }
    public int IdEntrenadorCreador { get; set; }
    public string NombreRutina { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Nivel { get; set; }
    public string? Objetivo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? NumeroDias { get; set; }
    public string? UrlImagen { get; set; }
    public List<RutinaDiaEjercicioResponse> DiasEjercicios { get; set; } = new();
}

public class RutinaDiaEjercicioResponse
{
    public int IdRutinaDiaEjercicio { get; set; }
    public int DiaNumero { get; set; }
    public int IdEjercicio { get; set; }
    public string EjercicioNombre { get; set; } = null!; // Include for better response
    public int OrdenEnDia { get; set; }
    public string? Series { get; set; }
    public string? Repeticiones { get; set; }
    public int? DescansoSegundos { get; set; }
    public string? NotasEjercicio { get; set; }
}