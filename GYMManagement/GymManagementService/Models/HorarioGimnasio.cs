using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementService.Models
{
    // Enum for DiaSemana (assuming MySQL ENUM maps well enough, or use string)
    public enum DiaSemana
    {
        Lunes, Martes, Miercoles, Jueves, Viernes, Sabado, Domingo
    }

    [Table("HorariosGimnasio")]
    public class HorarioGimnasio
    {
        [Key]
        [Column("id_horario_gimnasio")]
        public int IdHorarioGimnasio { get; set; }

        [Required]
        [Column("id_gimnasio")]
        public int IdGimnasio { get; set; } // Foreign Key property

        [Required]
        [Column("dia_semana")]
        [EnumDataType(typeof(DiaSemana))] // Helps with validation
        public DiaSemana DiaSemana { get; set; }

        [Required]
        [Column("hora_apertura")]
        public TimeOnly HoraApertura { get; set; } // .NET 6+ TimeOnly maps well to TIME

        [Required]
        [Column("hora_cierre")]
        public TimeOnly HoraCierre { get; set; }

        // Navigation Property to the parent Gym
        [ForeignKey("IdGimnasio")]
        public virtual Gimnasio? Gimnasio { get; set; }
    }
}