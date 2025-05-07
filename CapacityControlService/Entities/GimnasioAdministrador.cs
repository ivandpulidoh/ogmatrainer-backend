using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapacityControlService.Entities;

[Table("GimnasioAdministradores")]
[PrimaryKey(nameof(IdGimnasio), nameof(IdUsuario))] // Composite key
public class GimnasioAdministrador
{
    [Column("id_gimnasio")]
    public int IdGimnasio { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    // Navigation properties (optional)
    // public virtual Gimnasio Gimnasio { get; set; }
    // public virtual Usuario Usuario { get; set; }
}