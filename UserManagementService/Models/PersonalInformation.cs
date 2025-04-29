using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementService.Models
{
    [Table("PersonalInformation")]
    public class PersonalInformation
    {
        [Key] // Primary Key is also the Foreign Key
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("altura_cm", TypeName = "decimal(5, 1)")]
        public decimal? AlturaCm { get; set; }

        [Column("peso_inicial_kg", TypeName = "decimal(5, 2)")]
        public decimal? PesoInicialKg { get; set; }

        [Column("peso_actual_kg", TypeName = "decimal(5, 2)")]
        public decimal? PesoActualKg { get; set; }

        [Column("peso_objetivo_kg", TypeName = "decimal(5, 2)")]
        public decimal? PesoObjetivoKg { get; set; }

        [Column("objetivo_principal")]
        public string? ObjetivoPrincipal { get; set; } // TEXT maps to string

        [Column("experiencia_entrenamiento")]
        [MaxLength(20)] // Assuming max length for ENUM string representation
        public string? ExperienciaEntrenamiento { get; set; } // Store ENUM as string or map later

        [Column("nivel_actividad_diaria")]
        [MaxLength(20)]
        public string? NivelActividadDiaria { get; set; }

        [Column("condiciones_medicas")]
        public string? CondicionesMedicas { get; set; }

        [Column("disponibilidad_entrenamiento")]
        public string? DisponibilidadEntrenamiento { get; set; }

        [Column("preferencia_lugar_entrenamiento")]
        [MaxLength(20)]
        public string? PreferenciaLugarEntrenamiento { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_ultima_actualizacion")]
        public DateTime FechaUltimaActualizacion { get; set; } = DateTime.UtcNow;

        // Navigation Property (inverse relationship)
        [ForeignKey(nameof(IdUsuario))]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}