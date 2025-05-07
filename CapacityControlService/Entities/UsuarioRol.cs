using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapacityControlService.Entities;

[Table("UsuarioRoles")]
[PrimaryKey(nameof(IdUsuario), nameof(IdRol))] // Composite key
public class UsuarioRol
{
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("id_rol")]
    public int IdRol { get; set; }

    [ForeignKey("IdUsuario")]
    public virtual Usuario? Usuario { get; set; }

    [ForeignKey("IdRol")]
    public virtual Rol? Rol { get; set; }
}