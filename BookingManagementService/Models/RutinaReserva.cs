using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingManagementService.Entities;

[Table("RutinaReservas")]
public class RutinaReserva
{
    [Key]
    [Column("id_rutina_reserva")]
    public int IdRutinaReserva { get; set; }

    
    [Column("id_rutina_dia_ejercicio")]
    public int IdRutinaDiaEjercicio { get; set; }    

    [Column("id_reserva_espacio")]
    public int? IdReservaEspacio { get; set; }

    [Column("id_reserva_maquina")]
    public int? IdReservaMaquina { get; set; }

    [Column("id_reserva_entrenador")]
    public int? IdReservaEntrenador { get; set; }

    [Column("fecha_agrupacion")]
    public DateTime FechaAgrupacion { get; set; } = DateTime.UtcNow;

    [Column("notas")]
    public string? Notas { get; set; }
    
    //[ForeignKey("IdReservaEspacio")]
    //public virtual ReservaEspacio? ReservaEspacio { get; set; }

    [ForeignKey("IdReservaMaquina")]
    public virtual ReservaMaquina? ReservaMaquina { get; set; }

    [ForeignKey("IdReservaEntrenador")]
    public virtual ReservaEntrenador? ReservaEntrenador { get; set; }

}