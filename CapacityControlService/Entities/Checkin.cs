using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapacityControlService.Entities;

[Table("CheckIns")]
public class CheckIn
{
    [Key]
    [Column("id_checkin")]
    public int IdCheckin { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("id_gimnasio")]
    public int IdGimnasio { get; set; }

    [Column("hora_entrada")]
    public DateTime HoraEntrada { get; set; } = DateTime.UtcNow;

    [Column("hora_salida")]
    public DateTime? HoraSalida { get; set; }

    // Navigation properties (optional but helpful)
    [ForeignKey("IdUsuario")]
    public virtual Usuario? Usuario { get; set; }

    [ForeignKey("IdGimnasio")]
    public virtual Gimnasio? Gimnasio { get; set; }

    public virtual FormularioSintomas? FormularioSintomas { get; set; } // One-to-one link
}