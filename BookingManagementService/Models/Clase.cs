using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingManagementService.Entities;

[Table("Clases")]
public class Clase
{
    [Key]
    [Column("id_clase")]
    public int IdClase { get; set; }

    [Column("id_gimnasio")]
    public int IdGimnasio { get; set; }

    [Column("id_entrenador")]
    public int? IdEntrenador { get; set; }

    [Required]
    [Column("nombre_clase")]
    public string NombreClase { get; set; } = null!;

    [Required]
    [Column("tipo")] // EnVivo, Grabada
    public string Tipo { get; set; } = null!;

    [Column("fecha_hora_inicio")]
    public DateTime? FechaHoraInicio { get; set; }

    [Column("duracion_minutos")]
    public int DuracionMinutos {get; set;}

    [Column("capacidad_maxima")]
    public int? CapacidadMaxima { get; set; }

    [Column("activa")]
    public bool Activa { get; set; } = true;
}