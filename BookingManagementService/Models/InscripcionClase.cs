using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingManagementService.Entities;

[Table("InscripcionesClases")]
public class InscripcionClase
{
    [Key]
    [Column("id_inscripcion")]
    public int IdInscripcion { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("id_clase")]
    public int IdClase { get; set; }

    [Column("fecha_inscripcion")]
    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

    [Column("asistio")]
    public bool? Asistio { get; set; }

    // Navigation Properties
    [ForeignKey("IdUsuario")]
    public virtual Usuario Usuario { get; set; } = null!;

    [ForeignKey("IdClase")]
    public virtual Clase Clase { get; set; } = null!;
}