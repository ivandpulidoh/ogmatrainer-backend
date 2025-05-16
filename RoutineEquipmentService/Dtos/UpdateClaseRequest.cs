using System.ComponentModel.DataAnnotations;
namespace RoutineEquipmentService.Models;

public class UpdateClaseRequest
{    
    public int? IdGimnasio { get; set; }
    public int? IdEntrenador { get; set; }

    [StringLength(150, MinimumLength = 3)]
    public string? NombreClase { get; set; }

    public string? Descripcion { get; set; }

    [RegularExpression("^(EnVivo|Grabada)$", ErrorMessage = "Tipo must be 'EnVivo' or 'Grabada'.")]
    public string? Tipo { get; set; }

    [StringLength(255)]
    [Url(ErrorMessage = "Please enter a valid URL for UrlClase.")]
    public string? UrlClase { get; set; } 

    [StringLength(255)]
    [Url(ErrorMessage = "Please enter a valid URL for UrlImagen.")]
    public string? UrlImagen { get; set; }

    public DateTime? FechaHoraInicio { get; set; }
    public int? DuracionMinutos { get; set; }
    public int? CapacidadMaxima { get; set; }
    public bool? Activa { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DuracionMinutos.HasValue && DuracionMinutos.Value <= 0)
            yield return new ValidationResult("DuracionMinutos must be a positive number if provided.", new[] { nameof(DuracionMinutos) });        
    }
}