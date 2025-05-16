namespace RoutineEquipmentService.Models;

public class ClaseResponse
{
    public int IdClase { get; set; }
    public int IdGimnasio { get; set; }
    public int? IdEntrenador { get; set; }    
    public string NombreClase { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = null!;
    public string? UrlClase { get; set; }
    public string? UrlImagen { get; set; }
    public DateTime? FechaHoraInicio { get; set; }
    public int? DuracionMinutos { get; set; }
    public int? CapacidadMaxima { get; set; }
    public bool Activa { get; set; }
}