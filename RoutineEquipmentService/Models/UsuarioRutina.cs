using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RoutineEquipmentService.Models;

namespace RoutineEquipmentService.Entities;

[Table("UsuarioRutinas")]
public class UsuarioRutina
{
    [Key]
    [Column("id_usuario_rutina")]
    public int IdUsuarioRutina { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }         
    
    [Column("id_rutina")]
    public int IdRutina { get; set; }

    [Column("id_entrenador_asignador")]
    public int IdEntrenadorAsignador { get; set; }

    [Column("fecha_asignacion")]
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    [Column("activa")]
    public bool Activa { get; set; } = true;

    [ForeignKey("IdRutina")]
    public virtual Rutina? Rutina { get; set; }
}