namespace BookingManagementService.Models.External;

public class ExternalRutinaDiaEjercicioDto
{
    public int IdRutinaDiaEjercicio { get; set; }
    public int DiaNumero { get; set; }
    public int IdEjercicio { get; set; }
    public string? EjercicioNombre { get; set; } 
    public int OrdenEnDia { get; set; }
    public string? Series { get; set; }       
    public string? Repeticiones { get; set; } 
    public int? DescansoSegundos { get; set; }
    public string? NotasEjercicio { get; set; }
}

public class ExternalRoutineDto
{
    public int IdRutina { get; set; }
    public int IdEntrenadorCreador { get; set; }
    public string? NombreRutina { get; set; }
    public string? Descripcion { get; set; }    
    public string? Nivel { get; set; }          
    public string? Objetivo { get; set; }      
    public DateTime FechaCreacion { get; set; } 
    public int? NumeroDias { get; set; }         
    public string? UrlImagen { get; set; }      
    public List<ExternalRutinaDiaEjercicioDto> DiasEjercicios { get; set; } = new();
}