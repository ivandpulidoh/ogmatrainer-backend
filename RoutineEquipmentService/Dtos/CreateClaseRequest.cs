using System.ComponentModel.DataAnnotations;
namespace RoutineEquipmentService.Models;

public class CreateClaseRequest
{
    [Required]
    public int IdGimnasio { get; set; }
    public int? IdEntrenador { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string NombreClase { get; set; } = null!;

    public string? Descripcion { get; set; }

    [Required]
    [RegularExpression("^(EnVivo|Grabada)$", ErrorMessage = "Tipo must be 'EnVivo' or 'Grabada'.")]
    public string Tipo { get; set; } = null!;

    [StringLength(255)]
    [Url(ErrorMessage = "Please enter a valid URL for UrlClase.")]
    public string? UrlClase { get; set; }

    [StringLength(255)]
    [Url(ErrorMessage = "Please enter a valid URL for UrlImagen.")]
    public string? UrlImagen { get; set; }

    // For "EnVivo" classes, these are often required
    public DateTime? FechaHoraInicio { get; set; }
    public int? DuracionMinutos { get; set; }
    public int? CapacidadMaxima { get; set; }

    public bool Activa { get; set; } = true;

    // Custom validation (example)
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Tipo == "EnVivo")
        {
            if (!FechaHoraInicio.HasValue)
                yield return new ValidationResult("FechaHoraInicio is required for 'EnVivo' classes.", new[] { nameof(FechaHoraInicio) });
            if (!DuracionMinutos.HasValue || DuracionMinutos <= 0)
                yield return new ValidationResult("DuracionMinutos must be a positive number for 'EnVivo' classes.", new[] { nameof(DuracionMinutos) });
        }
        if (Tipo == "Grabada" && string.IsNullOrEmpty(UrlClase))
        {
             yield return new ValidationResult("UrlClase is required for 'Grabada' classes.", new[] { nameof(UrlClase) });
        }
    }
}