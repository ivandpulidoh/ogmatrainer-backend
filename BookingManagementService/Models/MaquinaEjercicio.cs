using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingManagementService.Entities;

[Table("MaquinasEjercicio")]
public class MaquinaEjercicio
{
    [Key]
    [Column("id_maquina")]
    public int IdMaquina { get; set; }

    [Column("id_espacio")]
    public int IdEspacio { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = null!;

    [Required]
    [Column("estado")]
    public string Estado { get; set; } = "Disponible"; // Disponible, EnMantenimiento, Averiada, Desactivada

    [Column("reservable")]
    public bool Reservable { get; set; } = true;

    // Navigation property (optional)
    // public virtual EspacioDeportivo Espacio { get; set; }
}