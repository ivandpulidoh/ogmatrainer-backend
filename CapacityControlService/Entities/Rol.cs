using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapacityControlService.Entities;
[Table("Roles")]
public class Rol
{
    [Key]
    [Column("id_rol")]
    public int IdRol { get; set; }
    [Required]
    [Column("nombre_rol")]
    public string NombreRol { get; set; } = null!; // Cliente, Entrenador, Administrador, AdminGimnasio
}