using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapacityControlService.Entities;

[Table("FormulariosSintomas")]
public class FormularioSintomas
{
    [Key]
    [Column("id_formulario")]
    public int IdFormulario { get; set; }

    // Foreign key to CheckIn (can also be the primary key if 1-to-1 is strict)
    [Required]
    [Column("id_checkin")]
    public int IdCheckin { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("fecha_envio")]
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

    [Column("tiene_sintomas")]
    public bool TieneSintomas { get; set; }

    [Column("tuvo_contacto_reciente")]
    public bool TuvoContactoReciente { get; set; }

    [Required]
    [Column("resultado_evaluacion")] // Aprobado, Rechazado
    public string ResultadoEvaluacion { get; set; } = null!;

    // Navigation properties
    [ForeignKey("IdCheckin")]
    public virtual CheckIn? CheckIn { get; set; }

    [ForeignKey("IdUsuario")]
    public virtual Usuario? Usuario { get; set; }
}