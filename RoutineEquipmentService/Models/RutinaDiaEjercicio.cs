using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoutineEquipmentService.Models;

[Table("RutinaDiaEjercicios")]
public class RutinaDiaEjercicio
{
    [Key]
    [Column("id_rutina_dia_ejercicio")]
    public int IdRutinaDiaEjercicio { get; set; }

    [Column("id_rutina")]
    public int IdRutina { get; set; }

    [Column("dia_numero")]
    public int DiaNumero { get; set; }

    [Column("id_ejercicio")]
    public int IdEjercicio { get; set; }

    [Column("orden_en_dia")]
    public int OrdenEnDia { get; set; }

    [MaxLength(20)]
    [Column("series")]
    public string? Series { get; set; }

    [MaxLength(20)]
    [Column("repeticiones")]
    public string? Repeticiones { get; set; }

    [Column("descanso_segundos")]
    public int? DescansoSegundos { get; set; }

    [Column("notas_ejercicio")]
    public string? NotasEjercicio { get; set; }

    // Navigation Properties
    [ForeignKey("IdRutina")]
    public virtual Rutina? Rutina { get; set; }

    [ForeignKey("IdEjercicio")]
    public virtual Ejercicio? Ejercicio { get; set; }
}