using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingManagementService.Entities;

[Table("ReservasEntrenador")]
public class ReservaEntrenador
{
    [Key]
    [Column("id_reserva_entrenador")]
    public int IdReservaEntrenador { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("id_entrenador")]
    public int IdEntrenador { get; set; }

    [Column("id_espacio")]
    public int? IdEspacio { get; set; }

    [Column("fecha_hora_inicio")]
    public DateTime FechaHoraInicio { get; set; }

    [Column("fecha_hora_fin")]
    public DateTime FechaHoraFin { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("estado")] // Confirmada, Cancelada, Completada, NoShowCliente, NoShowEntrenador
    public string Estado { get; set; } = "Confirmada";

    [Column("asistio_cliente")]
    public bool? AsistioCliente { get; set; }

    [Column("asistio_entrenador")]
    public bool? AsistioEntrenador { get; set; }

    // Navigation Properties
    [ForeignKey("IdCliente")]
    public virtual Usuario Cliente { get; set; } = null!;

    [ForeignKey("IdEntrenador")]
    public virtual Usuario Entrenador { get; set; } = null!;

    // Add Espacio if needed
}