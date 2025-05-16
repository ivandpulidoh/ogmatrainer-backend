using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoutineEquipmentService.Models;

[Table("MaquinasEjercicio")]
public class MaquinaEjercicio
{
    [Key]
    [Column("id_maquina")]
    public int IdMaquina { get; set; }

    [Column("id_espacio")]
    public int IdEspacio { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = null!;

    [MaxLength(100)]
    [Column("tipo_maquina")]
    public string? TipoMaquina { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("fecha_adquisicion")]
    public DateTime? FechaAdquisicion { get; set; }

    [Required]
    [MaxLength(20)] // Disponible, EnMantenimiento, Averiada, Desactivada
    [Column("estado")]
    public string Estado { get; set; } = "Disponible";

    [Column("reservable")]
    public bool Reservable { get; set; } = true;

    [Column("codigo_qr")]
    public byte[]? CodigoQr { get; set; }

    [MaxLength(255)]
    [Column("url_imagen")]
    public string? UrlImagen { get; set; }

    // Navigation Property
    [ForeignKey("IdEspacio")]
    public virtual EspacioDeportivo? EspacioDeportivo { get; set; }
}