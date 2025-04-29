namespace UserManagementService.Dtos
{
    // Can be used for both reading and updating
    public class PersonalInformationDto
    {
        public decimal? AlturaCm { get; set; }
        public decimal? PesoInicialKg { get; set; }
        public decimal? PesoActualKg { get; set; }
        public decimal? PesoObjetivoKg { get; set; }
        public string? ObjetivoPrincipal { get; set; }
        // Use string for ENUMs here, validate in service/controller
        public string? ExperienciaEntrenamiento { get; set; }
        public string? NivelActividadDiaria { get; set; }
        public string? CondicionesMedicas { get; set; }
        public string? DisponibilidadEntrenamiento { get; set; }
        public string? PreferenciaLugarEntrenamiento { get; set; }

        // You might want separate Create/Update DTOs if validation rules differ
    }
}