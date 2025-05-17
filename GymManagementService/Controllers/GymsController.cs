using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementService.Data;
using GymManagementService.Models;
using System.Collections.Generic; // For List/IEnumerable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // For [Authorize]
using Microsoft.AspNetCore.Http; // For IHttpContextAccessor
using System.Net.Http.Headers; 
using GymManagementService.DTOs;
using GymManagementService.Models.Settings;
using Microsoft.Extensions.Options;
using System.Web; // For async operations

namespace GymManagementService.Controllers
{
    [Route("api/[controller]")] // Base route: /api/gyms
    [ApiController]
    public class GymsController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExternalApiSettings _apiSettings;   
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GymsController(GymDbContext context, IHttpClientFactory httpClientFactory, IOptions<ExternalApiSettings> apiSettings, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/gyms
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Gimnasio>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Gimnasio>>> GetGimnasios()
        {
            // Eager load related data if needed using Include()
            // return await _context.Gimnasios.Include(g => g.Horarios).ToListAsync();
            return await _context.Gimnasios.ToListAsync();
        }

        // GET: api/gyms/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Gimnasio), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Gimnasio>> GetGimnasio(int id)
        {
            // Find the gym by ID, potentially including related data
            var gimnasio = await _context.Gimnasios
                                       // .Include(g => g.Horarios) // Uncomment to include operating hours
                                       // .Include(g => g.Administradores) // Uncomment to include admins
                                       // .Include(g => g.Entrenadores) // Uncomment to include trainers
                                       .FirstOrDefaultAsync(g => g.IdGimnasio == id);

            if (gimnasio == null)
            {
                return NotFound($"Gym with ID {id} not found.");
            }

            return Ok(gimnasio);
        }

        // POST: api/gyms
        [HttpPost]
        [ProducesResponseType(typeof(Gimnasio), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Gimnasio>> PostGimnasio([FromBody] Gimnasio gimnasio) // Use [FromBody]
        {
            if (!ModelState.IsValid) // Basic validation based on attributes
            {
                return BadRequest(ModelState);
            }

            // Check if gym with the same name already exists (based on UNIQUE constraint)
            if (await _context.Gimnasios.AnyAsync(g => g.Nombre == gimnasio.Nombre))
            {
                // Return a specific error for duplicate name
                ModelState.AddModelError("Nombre", $"A gym with the name '{gimnasio.Nombre}' already exists.");
                return Conflict(ModelState); // 409 Conflict is appropriate
            }


            _context.Gimnasios.Add(gimnasio);
            await _context.SaveChangesAsync(); // Save changes to the database

            if (string.IsNullOrWhiteSpace(_apiSettings.QrCodeGeneratorBaseUrl))
            {                
                return StatusCode(StatusCodes.Status500InternalServerError, "QR Code generator URL is not configured.");
            }
        
            string? accessToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                                    .FirstOrDefault()?.Split(" ").LastOrDefault();

            if (string.IsNullOrEmpty(accessToken))
            {                
                return Unauthorized("Authorization token not found in the request to this service.");
            }

            var httpClient = _httpClientFactory.CreateClient(); 

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);           

            string encodedGymId = HttpUtility.UrlEncode(gimnasio.IdGimnasio.ToString());
            string encodedGymName = HttpUtility.UrlEncode(gimnasio.Nombre);

            string qrApiUrlEntrada = $"{_apiSettings.QrCodeGeneratorBaseUrl}?Name={gimnasio.IdGimnasio}&Description={gimnasio.Nombre}";
            string qrApiUrlSalida = $"{_apiSettings.QrCodeGeneratorBaseUrl}?Name={gimnasio.IdGimnasio}&Description={gimnasio.Nombre}";

            Console.WriteLine(qrApiUrlEntrada);
            try
            {
                HttpResponseMessage responseEntrada = await httpClient.GetAsync(qrApiUrlEntrada);
                if (responseEntrada.IsSuccessStatusCode)
                {
                    gimnasio.CodigoQrEntrada = await responseEntrada.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    var errorContent = await responseEntrada.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error generating Entrada QR for Gym {gimnasio.Nombre} (ID: {gimnasio.IdGimnasio}): {responseEntrada.StatusCode} - {errorContent}");

                    return StatusCode(StatusCodes.Status206PartialContent, $"Gym registered successfully, but failed to generate QR code: {responseEntrada.ReasonPhrase}");
                }

                // Generar Salida QR
                HttpResponseMessage responseSalida = await httpClient.GetAsync(qrApiUrlSalida);
                if (responseSalida.IsSuccessStatusCode)
                {
                    gimnasio.CodigoQrSalida = await responseSalida.Content.ReadAsByteArrayAsync();
                }
                else
                {

                    var errorContent = await responseSalida.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error generating Salida QR for Gym {gimnasio.Nombre} (ID: {gimnasio.IdGimnasio}): {responseSalida.StatusCode} - {errorContent}");
                    return StatusCode(StatusCodes.Status206PartialContent, $"Gym registered successfully, but failed to generate QR code salida: {responseSalida.ReasonPhrase}");
                }

                _context.Entry(gimnasio).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (HttpRequestException ex)
            {
                // Log network or other HTTP request errors
                Console.WriteLine($"HttpRequestException while generating QR codes for Gym {gimnasio.Nombre}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while communicating with the QR code generation service.");
            }
            catch (Exception ex) // Catch any other unexpected errors during QR processing
            {
                Console.WriteLine($"Exception while processing QR codes for Gym {gimnasio.Nombre}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing QR codes.");
            }

            // Return 201 Created with the location and the created object
            return CreatedAtAction(nameof(GetGimnasio), new { id = gimnasio.IdGimnasio }, gimnasio);
        }

        // PUT: api/gyms/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutGimnasio(int id, [FromBody] Gimnasio gimnasio)
        {
            if (id != gimnasio.IdGimnasio)
            {
                return BadRequest("ID mismatch between route parameter and request body.");
            }

             // Check if gym with the updated name already exists (and it's not the current gym)
             if (await _context.Gimnasios.AnyAsync(g => g.Nombre == gimnasio.Nombre && g.IdGimnasio != id))
             {
                 ModelState.AddModelError("Nombre", $"Another gym with the name '{gimnasio.Nombre}' already exists.");
                 return Conflict(ModelState); // 409 Conflict
             }


            _context.Entry(gimnasio).State = EntityState.Modified; // Mark the entity as modified

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) // Handle case where the gym was deleted concurrently
            {
                if (!await GimnasioExistsAsync(id))
                {
                    return NotFound($"Gym with ID {id} not found.");
                }
                else
                {
                    throw; // Re-throw other concurrency issues
                }
            }

            return NoContent(); // Standard response for successful PUT
        }

        // DELETE: api/gyms/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGimnasio(int id)
        {
            var gimnasio = await _context.Gimnasios.FindAsync(id);
            if (gimnasio == null)
            {
                return NotFound($"Gym with ID {id} not found.");
            }

            _context.Gimnasios.Remove(gimnasio);
            await _context.SaveChangesAsync(); // Cascade deletes should handle related data based on schema/config

            return NoContent(); // Standard response for successful DELETE
        }

        // --- Association Management ---

        // POST: api/gyms/5/admins
        [HttpPost("{gymId}/admins")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Or 201 if creating a resource link
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAdminToGym(int gymId, [FromBody] UserIdRequestDto request)
        {   
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            int userId = request.UserId;

            if (!await GimnasioExistsAsync(gymId))
                return NotFound($"Gym with ID {gymId} not found.");           

            var association = new GimnasioAdministrador { IdGimnasio = gymId, IdUsuario = userId };

            // Check if association already exists
            if (await _context.GimnasioAdministradores.AnyAsync(ga => ga.IdGimnasio == gymId && ga.IdUsuario == userId))
            {
                return Ok("User is already an administrator for this gym."); // Or Conflict(409)
            }

            _context.GimnasioAdministradores.Add(association);
            await _context.SaveChangesAsync();

            return Ok($"User {userId} added as administrator to Gym {gymId}");
        }


        // DELETE: api/gyms/5/admins/10
        [HttpDelete("{gymId}/admins/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveAdminFromGym(int gymId, int userId)
        {
             var association = await _context.GimnasioAdministradores
                .FirstOrDefaultAsync(ga => ga.IdGimnasio == gymId && ga.IdUsuario == userId);

            if (association == null)
            {
                return NotFound($"Administrator association not found for Gym {gymId} and User {userId}.");
            }

            _context.GimnasioAdministradores.Remove(association);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // POST: api/gyms/{gymId}/trainers
        [HttpPost("{gymId}/trainers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddTrainerToGym(int gymId, [FromBody] UserIdRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int userId = request.UserId;

            if (!await GimnasioExistsAsync(gymId))
                return NotFound($"Gym with ID {gymId} not found.");                       

            var association = new EntrenadorGimnasio { IdGimnasio = gymId, IdUsuario = userId };

            if (await _context.EntrenadorGimnasios.AnyAsync(eg => eg.IdGimnasio == gymId && eg.IdUsuario == userId))
            {
                return Ok("User is already a trainer for this gym."); // Or Conflict(409)
            }

            _context.EntrenadorGimnasios.Add(association);
            await _context.SaveChangesAsync();
            return Ok($"User {userId} added as trainer to Gym {gymId}");
        }

        // DELETE: api/gyms/{gymId}/trainers/{userId}
        [HttpDelete("{gymId}/trainers/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveTrainerFromGym(int gymId, int userId)
        {
             var association = await _context.EntrenadorGimnasios
                .FirstOrDefaultAsync(eg => eg.IdGimnasio == gymId && eg.IdUsuario == userId);

            if (association == null)
            {
                return NotFound($"Trainer association not found for Gym {gymId} and User {userId}.");
            }
            _context.EntrenadorGimnasios.Remove(association);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //GET: api/gyms/{gymId}/trainers
        [HttpGet("{gymId}/trainers")]
        [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<int>>> GetGymTrainers(int gymId)
        {
            // 1. Check if the gym exists
            if (!await GimnasioExistsAsync(gymId)) // Use async version
            {
                return NotFound($"Gym with ID {gymId} not found.");
            }

            // 2. Query the EntrenadorGimnasios table
            var trainerIds = await _context.EntrenadorGimnasios
                                       .Where(eg => eg.IdGimnasio == gymId) // Filter by gym ID
                                       .Select(eg => eg.IdUsuario)        // Select only the user ID
                                       .ToListAsync();                     // Execute the query

            // 3. Return the list of IDs
            return Ok(trainerIds);
        }


        // GET: api/gyms/{gymId}/admins
        [HttpGet("{gymId}/admins")]
        [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<int>>> GetGymAdministrators(int gymId)
        {
            
            if (!await GimnasioExistsAsync(gymId))
            {
                return NotFound($"Gym with ID {gymId} not found.");
            }

           
            var adminIds = await _context.GimnasioAdministradores
                                     .Where(ga => ga.IdGimnasio == gymId) 
                                     .Select(ga => ga.IdUsuario)       
                                     .ToListAsync();                     

            
            return Ok(adminIds);
        }
        
        private async Task<bool> GimnasioExistsAsync(int id)
        {
            return await _context.Gimnasios.AnyAsync(e => e.IdGimnasio == id);
        }
        
    }
}