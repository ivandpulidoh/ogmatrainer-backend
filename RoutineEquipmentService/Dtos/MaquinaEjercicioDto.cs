using System.ComponentModel.DataAnnotations;
namespace RoutineEquipmentService.Dtos;

public class CreateMaquinaRequest
{
    [Required]
    public int IdEspacio { get; set; }
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = null!;
    [MaxLength(100)]
    public string? TipoMaquina { get; set; }
    public string? Descripcion { get; set; } // Will be used for QR
    public DateTime? FechaAdquisicion { get; set; }
    [MaxLength(20)]
    public string Estado { get; set; } = "Disponible";
    public bool Reservable { get; set; } = true;
}

public class UpdateMaquinaRequest
{
    // Only include fields that can be updated
    public int? IdEspacio { get; set; } // Optional if you allow moving
    [MaxLength(100)]
    public string? Nombre { get; set; }
    [MaxLength(100)]
    public string? TipoMaquina { get; set; }
    public string? Descripcion { get; set; }
    public DateTime? FechaAdquisicion { get; set; }
    [MaxLength(20)]
    public string? Estado { get; set; }
    public bool? Reservable { get; set; }
}

public class MaquinaResponse
{
    public int IdMaquina { get; set; }
    public int IdEspacio { get; set; }
    public string Nombre { get; set; } = null!;
    public string? TipoMaquina { get; set; }
    public string? Descripcion { get; set; }
    public DateTime? FechaAdquisicion { get; set; }
    public string Estado { get; set; } = null!;
    public bool Reservable { get; set; }
    public string? CodigoQrBase64 { get; set; } // Represent QR as Base64 string for client
}