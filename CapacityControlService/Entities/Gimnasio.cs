using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapacityControlService.Entities;

[Table("Gimnasios")]
public class Gimnasio
{
    [Key]
    [Column("id_gimnasio")]
    public int IdGimnasio { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = null!;

    [Column("capacidad_maxima")]
    public int CapacidadMaxima { get; set; } = 100;

}