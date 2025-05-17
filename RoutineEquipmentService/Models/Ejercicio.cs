using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoutineEquipmentService.Models;

[Table("Ejercicios")]
public class Ejercicio
{
    [Key]
    [Column("id_ejercicio")]
    public int IdEjercicio { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("nombre")]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [MaxLength(100)]
    [Column("musculo_objetivo")]
    public string? MusculoObjetivo { get; set; }

    [MaxLength(255)]
    [Column("url_video_demostracion")]
    public string? UrlVideoDemostracion { get; set; }

    [Column("id_creador")]
    public int? IdCreador { get; set; }

    public virtual ICollection<EjercicioMaquina> MaquinasRequeridas { get; set; } = new List<EjercicioMaquina>();
}