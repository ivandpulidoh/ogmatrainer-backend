namespace RoutineEquipmentService.Dtos;

public class RutinaMaquinaRequirementDto
{
    public int IdEjercicio { get; set; }
    public string EjercicioNombre { get; set; } = null!;
    public int IdMaquina { get; set; }
    public string MaquinaNombre { get; set; } = null!;
    public string? MaquinaUrlImagen { get; set; }
    public string? NotasUnion { get; set; }
}