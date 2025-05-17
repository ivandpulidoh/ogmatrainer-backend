using MembershipService.Models; // Para EstadoMembresia

namespace MembershipService.DTOs
{
    public class MembresiaDto
    {
        public int IdMembresia { get; set; }
        public int IdUsuario { get; set; }
        public int IdTipoMembresia { get; set; }
        public string? NombreTipoMembresia { get; set; } // Para mostrar
        public int? IdGimnasioPrincipal { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public bool AutoRenovar { get; set; }
    }

    public class CreateMembresiaDto
    {
        public int IdUsuario { get; set; }
        public int IdTipoMembresia { get; set; }
        public int? IdGimnasioPrincipal { get; set; }
        public DateOnly FechaInicio { get; set; }
        // FechaFin se calculará
        public string Estado { get; set; } = EstadoMembresia.PendientePago.ToString(); // Valor por defecto
        public bool AutoRenovar { get; set; } = false;
    }

    public class UpdateMembresiaDto
    {
        // ¿Qué se puede actualizar? Estado, AutoRenovar
        public string? Estado { get; set; }
        public bool? AutoRenovar { get; set; }
        public DateOnly? FechaFin { get; set; } // Permitir extensión manual por un admin
    }
}