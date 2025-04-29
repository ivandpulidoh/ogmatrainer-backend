using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingManagementService.Entities;

[Table("ReservasMaquinas")]
public class ReservaMaquina
{
    [Key]
    [Column("id_reserva_maquina")]
    public int IdReservaMaquina { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("id_maquina")]
    public int IdMaquina { get; set; }

    [Column("fecha_hora_inicio")]
    public DateTime FechaHoraInicio { get; set; }

    [Column("fecha_hora_fin")]
    public DateTime FechaHoraFin { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // Use UtcNow generally

    [Required]
    [Column("estado")] // Confirmada, Cancelada, Completada, NoShow
    public string Estado { get; set; } = "Confirmada";

    [Column("asistio")]
    public bool? Asistio { get; set; } // Nullable bool

    [Column("notificacion_fin_enviada")] // This field from schema seems less relevant than 'asistio' for the NoShow logic
    public bool NotificacionFinEnviada { get; set; } = false;


    // Navigation properties
    [ForeignKey("IdUsuario")]
    public virtual Usuario Usuario { get; set; } = null!;

    [ForeignKey("IdMaquina")]
    public virtual MaquinaEjercicio MaquinaEjercicio { get; set; } = null!;
}