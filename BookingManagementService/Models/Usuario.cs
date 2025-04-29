using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingManagementService.Entities;

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

    [Required]
    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("activo")]
    public bool Activo { get; set; } = true;

}