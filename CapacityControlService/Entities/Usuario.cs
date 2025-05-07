using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapacityControlService.Entities;

[Table("Usuarios")]
public class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }
    [Column("nombre")]
    public string? Nombre { get; set; }
     [Column("apellido")]
    public string? Apellido { get; set; }
    [Column("email")]
    public string? Email { get; set; }
    // Include roles if needed for admin check
    public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}