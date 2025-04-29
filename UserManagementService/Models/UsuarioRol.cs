using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Required for [PrimaryKey] attribute

namespace UserManagementService.Models
{
    [Table("UsuarioRoles")]
    [PrimaryKey(nameof(IdUsuario), nameof(IdRol))] // Composite Primary Key
    public class UsuarioRol
    {
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_rol")]
        public int IdRol { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(IdUsuario))]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(IdRol))]
        public virtual Rol Rol { get; set; } = null!;
    }
}