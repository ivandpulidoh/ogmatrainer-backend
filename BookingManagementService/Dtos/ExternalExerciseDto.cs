namespace BookingManagementService.Models.External;

public class ExternalSimpleMaquinaDto
{
    public int IdMaquina { get; set; }
    public string? Nombre { get; set; }
    public string? UrlImagen { get; set; }
}

public class ExternalExerciseDto
{
    public int IdEjercicio { get; set; }
    public string? Nombre { get; set; }
    public List<ExternalSimpleMaquinaDto> MaquinasAsociadas { get; set; } = new();
}