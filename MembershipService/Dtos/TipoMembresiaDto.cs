namespace MembershipService.DTOs
{
    public class TipoMembresiaDto
    {
        public int IdTipoMembresia { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int DuracionMeses { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
    }

    public class CreateTipoMembresiaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int DuracionMeses { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class UpdateTipoMembresiaDto
    {
        public string? Nombre { get; set; } // Opcional para actualizar
        public string? Descripcion { get; set; }
        public int? DuracionMeses { get; set; }
        public decimal? Precio { get; set; }
        public bool? Activo { get; set; }
    }
}