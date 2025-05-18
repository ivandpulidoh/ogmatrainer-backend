namespace BookingManagementService.Models.External; // Use a sub-namespace

public class ExternalRutinaDiaEjercicioDto
{
    public int IdRutinaDiaEjercicio { get; set; }
    public int DiaNumero { get; set; }
    public int IdEjercicio { get; set; }
    public string? EjercicioNombre { get; set; } // Nullable to handle potential missing data
    public int OrdenEnDia { get; set; }
    public string? Series { get; set; }        // For duration estimation
    public string? Repeticiones { get; set; }  // For duration estimation
    public int? DescansoSegundos { get; set; } // For duration estimation
    public string? NotasEjercicio { get; set; }
}

public class ExternalRoutineDto
{
    public int IdRutina { get; set; }
    public string? NombreRutina { get; set; }
    public int NumeroDias { get; set; } // Important for validating DiaNumero
    public List<ExternalRutinaDiaEjercicioDto> DiasEjercicios { get; set; } = new();
}