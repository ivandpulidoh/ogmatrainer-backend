namespace RoutineEquipmentService.Dtos;

public class SimpleMaquinaDto
{
    public int IdMaquina { get; set; }
    public string Nombre { get; set; } = null!;
    public string? TipoMaquina { get; set; }
    public string? UrlImagen { get; set; }
    public string Estado { get; set; } = null!;
    public bool Reservable { get; set; } 
}