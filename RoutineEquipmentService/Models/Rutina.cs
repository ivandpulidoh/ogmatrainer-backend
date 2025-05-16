using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoutineEquipmentService.Models;

[Table("Rutinas")]
public class Rutina
{
    [Key]
    [Column("id_rutina")]
    public int IdRutina { get; set; }

    [Column("id_entrenador_creador")]
    public int IdEntrenadorCreador { get; set; } // FK to Usuarios

    [Required]
    [MaxLength(150)]
    [Column("nombre_rutina")]
    public string NombreRutina { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [MaxLength(15)] // Principiante, Intermedio, Avanzado
    [Column("nivel")]
    public string? Nivel { get; set; }

    [MaxLength(100)]
    [Column("objetivo")]
    public string? Objetivo { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Column("numero_dias")] // New field
    public int? NumeroDias { get; set; }

    [Column("url_imagen")]
    public string? UrlImagen { get; set; }

    // Navigation Property
    public virtual ICollection<RutinaDiaEjercicio> DiasEjercicios { get; set; } = new List<RutinaDiaEjercicio>();
}