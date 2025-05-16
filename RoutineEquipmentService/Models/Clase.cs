using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoutineEquipmentService.Models;

[Table("Clases")]
public class Clase
{
    [Key]
    [Column("id_clase")]
    public int IdClase { get; set; }

    [Column("id_gimnasio")]
    public int IdGimnasio { get; set; } // FK to Gimnasios

    [Column("id_entrenador")]
    public int? IdEntrenador { get; set; } // FK to Usuarios (Trainer role)

    [Required]
    [MaxLength(150)]
    [Column("nombre_clase")]
    public string NombreClase { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Required]
    [MaxLength(10)] // EnVivo, Grabada
    [Column("tipo")]
    public string Tipo { get; set; } = null!;

    [MaxLength(255)]
    [Column("url_clase")]
    public string? UrlClase { get; set; } // URL for live stream or recorded video

    [Column("fecha_hora_inicio")]
    public DateTime? FechaHoraInicio { get; set; } // Required for live classes

    [Column("duracion_minutos")]
    public int? DuracionMinutos { get; set; }

    [Column("capacidad_maxima")]
    public int? CapacidadMaxima { get; set; } // For live classes with limited spots

    [Column("activa")]
    public bool Activa { get; set; } = true;

    [MaxLength(255)]
    [Column("url_imagen")]
    public string? UrlImagen { get; set; }
}