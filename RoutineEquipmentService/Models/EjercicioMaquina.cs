using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoutineEquipmentService.Models;

[Table("EjercicioMaquinas")]
public class EjercicioMaquina
{
    [Key, Column("id_ejercicio", Order = 0)]
    public int IdEjercicio { get; set; }

    [Key, Column("id_maquina", Order = 1)]
    public int IdMaquina { get; set; }

    [MaxLength(255)]
    [Column("notas")]
    public string? Notas { get; set; }

    // Navigation Properties
    [ForeignKey("IdEjercicio")]
    public virtual Ejercicio? Ejercicio { get; set; }

    [ForeignKey("IdMaquina")]
    public virtual MaquinaEjercicio? MaquinaEjercicio { get; set; }
}