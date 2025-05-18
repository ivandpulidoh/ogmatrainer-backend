using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoutineEquipmentService.Models;

[Table("EspaciosDeportivos")]
public class EspacioDeportivo
{
    [Key]
    [Column("id_espacio")]
    public int IdEspacio { get; set; }

    [Column("id_gimnasio")]
    public int IdGimnasio { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nombre_espacio")]
    public string NombreEspacio { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("capacidad")]
    public int Capacidad { get; set; } = 1;

    [Column("reservable")]
    public bool Reservable { get; set; } = true;

     public virtual ICollection<MaquinaEjercicio> MaquinasEnEspacio { get; set; } = new List<MaquinaEjercicio>();
}