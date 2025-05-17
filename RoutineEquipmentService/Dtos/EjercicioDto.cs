using System.ComponentModel.DataAnnotations;
namespace RoutineEquipmentService.Dtos;

public class CreateEjercicioRequest
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    [MaxLength(100)]
    public string? MusculoObjetivo { get; set; }
    [MaxLength(255)]
    public string? UrlVideoDemostracion { get; set; }
    public List<int>? MaquinasRequeridasIds { get; set; } = new List<int>();
}
public class UpdateEjercicioRequest : CreateEjercicioRequest {} // Can inherit for updates

public class EjercicioResponse
{
    public int IdEjercicio { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? MusculoObjetivo { get; set; }
    public string? UrlVideoDemostracion { get; set; }
    public int? IdCreador { get; set; }
    public List<SimpleMaquinaDto> MaquinasAsociadas { get; set; } = new List<SimpleMaquinaDto>();
}