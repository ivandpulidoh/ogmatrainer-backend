using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UserManagementService.Data;
using UserManagementService.Dtos;
using UserManagementService.Models;

namespace UserManagementService.Services
{
    public class PersonalInformationService : IPersonalInformationService
    {
        private readonly UserManagementDbContext _context;
        private readonly ILogger<PersonalInformationService> _logger;

        public PersonalInformationService(UserManagementDbContext context, ILogger<PersonalInformationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PersonalInformationDto?> GetPersonalInformationAsync(int userId)
        {
            _logger.LogInformation("Attempting to retrieve personal information for User ID: {UserId}", userId);
            var personalInfo = await _context.PersonalInformation
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.IdUsuario == userId);

            if (personalInfo == null)
            {
                _logger.LogWarning("Personal information not found for User ID: {UserId}", userId);
                return null;
            }

            return MapEntityToDto(personalInfo);
        }

        public async Task<bool> UpsertPersonalInformationAsync(int userId, PersonalInformationDto piDto)
        {
            _logger.LogInformation("Attempting to upsert personal information for User ID: {UserId}", userId);

            // First, ensure the user exists and is active (optional, but good practice)
            var userExists = await _context.Usuarios.AnyAsync(u => u.IdUsuario == userId && u.Activo);
            if (!userExists)
            {
                 _logger.LogWarning("Upsert failed: User with ID {UserId} not found or is inactive.", userId);
                 return false;
            }


            var existingInfo = await _context.PersonalInformation
                                            .FirstOrDefaultAsync(pi => pi.IdUsuario == userId);

            if (existingInfo == null)
            {
                // Create new PersonalInformation record
                 _logger.LogInformation("Creating new personal information record for User ID: {UserId}", userId);
                var newInfo = new PersonalInformation
                {
                    IdUsuario = userId, // Link to the user
                    FechaCreacion = DateTime.UtcNow,
                    FechaUltimaActualizacion = DateTime.UtcNow
                    // Map other fields from DTO
                };
                MapDtoToEntity(piDto, newInfo); // Use helper to map DTO fields
                _context.PersonalInformation.Add(newInfo);
            }
            else
            {
                // Update existing PersonalInformation record
                 _logger.LogInformation("Updating existing personal information record for User ID: {UserId}", userId);
                MapDtoToEntity(piDto, existingInfo); // Update fields from DTO
                existingInfo.FechaUltimaActualizacion = DateTime.UtcNow;
                _context.PersonalInformation.Update(existingInfo); // Explicitly mark as updated (optional depending on tracking)
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully upserted personal information for User ID: {UserId}", userId);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during upsert of personal information for User ID: {UserId}", userId);
                return false;
            }
        }

        // --- Private Helper Methods ---

        private PersonalInformationDto MapEntityToDto(PersonalInformation entity)
        {
            return new PersonalInformationDto
            {
                AlturaCm = entity.AlturaCm,
                PesoInicialKg = entity.PesoInicialKg,
                PesoActualKg = entity.PesoActualKg,
                PesoObjetivoKg = entity.PesoObjetivoKg,
                ObjetivoPrincipal = entity.ObjetivoPrincipal,
                ExperienciaEntrenamiento = entity.ExperienciaEntrenamiento,
                NivelActividadDiaria = entity.NivelActividadDiaria,
                CondicionesMedicas = entity.CondicionesMedicas,
                DisponibilidadEntrenamiento = entity.DisponibilidadEntrenamiento,
                PreferenciaLugarEntrenamiento = entity.PreferenciaLugarEntrenamiento
                // Note: Timestamps usually not included in DTOs unless specifically needed by client
            };
        }

        private void MapDtoToEntity(PersonalInformationDto dto, PersonalInformation entity)
        {
            // Map all properties from DTO to the Entity
            entity.AlturaCm = dto.AlturaCm;
            entity.PesoInicialKg = dto.PesoInicialKg;
            entity.PesoActualKg = dto.PesoActualKg;
            entity.PesoObjetivoKg = dto.PesoObjetivoKg;
            entity.ObjetivoPrincipal = dto.ObjetivoPrincipal;
            // Add validation or mapping for ENUM-like strings if needed
            entity.ExperienciaEntrenamiento = dto.ExperienciaEntrenamiento;
            entity.NivelActividadDiaria = dto.NivelActividadDiaria;
            entity.CondicionesMedicas = dto.CondicionesMedicas;
            entity.DisponibilidadEntrenamiento = dto.DisponibilidadEntrenamiento;
            entity.PreferenciaLugarEntrenamiento = dto.PreferenciaLugarEntrenamiento;
        }
    }
}