using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementService.Data;
using GymManagementService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagementService.Controllers
{
    // Route: /api/gyms/{gymId}/gymhours
    [Route("api/gyms/{gymId}/[controller]")]
    [ApiController]
    public class GymHoursController : ControllerBase
    {
        private readonly GymDbContext _context;

        // Inject the DbContext
        public GymHoursController(GymDbContext context)
        {
            _context = context;
        }

        // Helper method to check if Gym exists (avoids repetition)
        private async Task<bool> GymExistsAsync(int gymId)
        {
            return await _context.Gimnasios.AnyAsync(g => g.IdGimnasio == gymId);
        }

        // GET: api/gyms/{gymId}/gymhours
        /// <summary>
        /// Gets all operating hours for a specific gym.
        /// </summary>
        /// <param name="gymId">The ID of the gym.</param>
        /// <returns>A list of operating hours.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<HorarioGimnasio>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<HorarioGimnasio>>> GetGymHours(int gymId)
        {
            if (!await GymExistsAsync(gymId))
            {
                return NotFound($"Gym with ID {gymId} not found.");
            }

            var hours = await _context.HorariosGimnasio
                                    .Where(h => h.IdGimnasio == gymId)
                                    .OrderBy(h => h.DiaSemana) // Optional: Order by day
                                    .ToListAsync();

            return Ok(hours);
        }

        // GET: api/gyms/{gymId}/gymhours/{hourId}
        /// <summary>
        /// Gets a specific operating hour record by its ID for a specific gym.
        /// </summary>
        /// <param name="gymId">The ID of the gym.</param>
        /// <param name="hourId">The ID of the operating hour record.</param>
        /// <returns>The requested operating hour record.</returns>
        [HttpGet("{hourId}")]
        [ProducesResponseType(typeof(HorarioGimnasio), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HorarioGimnasio>> GetGymHour(int gymId, int hourId)
        {
            // Query checking both gymId and hourId for correctness
            var horario = await _context.HorariosGimnasio
                                      .FirstOrDefaultAsync(h => h.IdGimnasio == gymId && h.IdHorarioGimnasio == hourId);

            if (horario == null)
            {
                // Be specific: Is the Gym missing or the Hour record?
                // The query already checks gymId implicitly, so if null, the hour record for this gym wasn't found.
                return NotFound($"Operating hour record with ID {hourId} not found for Gym ID {gymId}.");
            }

            return Ok(horario);
        }

        // POST: api/gyms/{gymId}/gymhours
        /// <summary>
        /// Creates a new operating hour record for a specific gym.
        /// </summary>
        /// <param name="gymId">The ID of the gym.</param>
        /// <param name="horarioGimnasio">The operating hour details.</param>
        /// <returns>The created operating hour record.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(HorarioGimnasio), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // For unique constraint violation
        public async Task<ActionResult<HorarioGimnasio>> PostGymHour(int gymId, [FromBody] HorarioGimnasio horarioGimnasio)
        {
            // 1. Basic Model Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Check if the associated Gym exists
            if (!await GymExistsAsync(gymId))
            {
                return NotFound($"Gym with ID {gymId} not found. Cannot add hours to a non-existent gym.");
            }

            // 3. Ensure the FK is set correctly from the route parameter
            horarioGimnasio.IdGimnasio = gymId;

            // 4. Check for Unique Constraint (uk_gimnasio_dia) violation BEFORE adding
            bool alreadyExists = await _context.HorariosGimnasio
                .AnyAsync(h => h.IdGimnasio == gymId && h.DiaSemana == horarioGimnasio.DiaSemana);

            if (alreadyExists)
            {
                return Conflict($"An operating hour record already exists for Gym ID {gymId} on {horarioGimnasio.DiaSemana}. Use PUT to modify.");
            }

            // 5. Add and Save
            _context.HorariosGimnasio.Add(horarioGimnasio);
            await _context.SaveChangesAsync();

            // 6. Return 201 Created response
            // Use nameof(GetGymHour) for the location header
            return CreatedAtAction(nameof(GetGymHour),
                                   new { gymId = gymId, hourId = horarioGimnasio.IdHorarioGimnasio },
                                   horarioGimnasio);
        }

        // PUT: api/gyms/{gymId}/gymhours/{hourId}
        /// <summary>
        /// Updates an existing operating hour record for a specific gym.
        /// </summary>
        /// <param name="gymId">The ID of the gym.</param>
        /// <param name="hourId">The ID of the operating hour record to update.</param>
        /// <param name="horarioGimnasio">The updated operating hour details.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{hourId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // If changing DiaSemana conflicts
        public async Task<IActionResult> PutGymHour(int gymId, int hourId, [FromBody] HorarioGimnasio horarioGimnasio)
        {
            // 1. ID Mismatch Check
            if (hourId != horarioGimnasio.IdHorarioGimnasio)
            {
                return BadRequest("ID mismatch between route parameter and request body.");
            }

            // 2. Basic Model Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 3. Prevent changing the Gym ID via PUT on a nested resource
            if (gymId != horarioGimnasio.IdGimnasio)
            {
                 // Or just ignore horarioGimnasio.IdGimnasio and trust gymId from route? Safer to reject.
                return BadRequest("Cannot change the Gym ID for an existing hour record via this endpoint.");
            }

            // 4. Check for potential Unique Constraint violation if DiaSemana is changing
            bool changingDay = await _context.HorariosGimnasio
                .AnyAsync(h => h.IdHorarioGimnasio == hourId && h.DiaSemana != horarioGimnasio.DiaSemana);

            if (changingDay)
            {
                bool newDayConflict = await _context.HorariosGimnasio
                    .AnyAsync(h => h.IdGimnasio == gymId &&
                                   h.DiaSemana == horarioGimnasio.DiaSemana &&
                                   h.IdHorarioGimnasio != hourId); // Exclude self
                if (newDayConflict)
                {
                     return Conflict($"An operating hour record already exists for Gym ID {gymId} on {horarioGimnasio.DiaSemana}. Cannot update to this day.");
                }
            }

            // 5. Mark entity as modified
            _context.Entry(horarioGimnasio).State = EntityState.Modified;

            try
            {
                // 6. Attempt to Save
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // 7. Handle Concurrency / Not Found during save
                if (!await _context.HorariosGimnasio.AnyAsync(h => h.IdHorarioGimnasio == hourId && h.IdGimnasio == gymId))
                {
                    return NotFound($"Operating hour record with ID {hourId} not found for Gym ID {gymId}.");
                }
                else
                {
                    throw; // Re-throw if it's a different concurrency issue
                }
            }

            // 8. Return 204 No Content on success
            return NoContent();
        }

        // DELETE: api/gyms/{gymId}/gymhours/{hourId}
        /// <summary>
        /// Deletes a specific operating hour record for a gym.
        /// </summary>
        /// <param name="gymId">The ID of the gym.</param>
        /// <param name="hourId">The ID of the operating hour record to delete.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{hourId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGymHour(int gymId, int hourId)
        {
            // 1. Find the specific record ensuring it belongs to the correct gym
            var horarioGimnasio = await _context.HorariosGimnasio
                                            .FirstOrDefaultAsync(h => h.IdGimnasio == gymId && h.IdHorarioGimnasio == hourId);

            // 2. Check if found
            if (horarioGimnasio == null)
            {
                return NotFound($"Operating hour record with ID {hourId} not found for Gym ID {gymId}.");
            }

            // 3. Remove and Save
            _context.HorariosGimnasio.Remove(horarioGimnasio);
            await _context.SaveChangesAsync();

            // 4. Return 204 No Content
            return NoContent();
        }
    }
}