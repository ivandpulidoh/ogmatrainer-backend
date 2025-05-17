using MembershipService.DTOs;
using MembershipService.Interfaces;
using MembershipService.Services;
using Microsoft.AspNetCore.Mvc;
using System; // Para DateOnly en .NET 6+
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MembershipService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembresiasController : ControllerBase
    {
        private readonly IMembresiaService _service;
        private readonly ILogger<MembresiasController> _logger;


        public MembresiasController(IMembresiaService service, ILogger<MembresiasController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembresiaDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MembresiaDto>> GetById(int id)
        {
            var membresia = await _service.GetByIdAsync(id);
            if (membresia == null) return NotFound();
            return Ok(membresia);
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<MembresiaDto>>> GetByUsuarioId(int usuarioId)
        {
            return Ok(await _service.GetByUsuarioIdAsync(usuarioId));
        }

        [HttpPost]
        public async Task<ActionResult<MembresiaDto>> Create(CreateMembresiaDto createDto)
        {
            try
            {
                var nuevaMembresia = await _service.CreateAsync(createDto);
                 if (nuevaMembresia == null) return BadRequest("No se pudo crear la membresía, verifique los datos.");
                return CreatedAtAction(nameof(GetById), new { id = nuevaMembresia.IdMembresia }, nuevaMembresia);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de argumento al crear membresía: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error inesperado al crear membresía.");
                return StatusCode(500, "Ocurrió un error interno al procesar la solicitud.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateMembresiaDto updateDto)
        {
            try
            {
                var success = await _service.UpdateAsync(id, updateDto);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                 _logger.LogWarning(ex, "Error de argumento al actualizar membresía: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error inesperado al actualizar membresía.");
                return StatusCode(500, "Ocurrió un error interno al procesar la solicitud.");
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancel(int id)
        {
            var success = await _service.CancelAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Membresía cancelada exitosamente." });
        }
        
        public record RenewRequest(DateOnly NuevaFechaInicio); // Record para el body del request

        [HttpPost("{id}/renovar")]
        public async Task<ActionResult<MembresiaDto>> Renew(int id, [FromBody] RenewRequest request)
        {
            if (request == null || request.NuevaFechaInicio == default)
            {
                return BadRequest("Se requiere la nueva fecha de inicio para la renovación.");
            }

            var membresiaRenovada = await _service.RenewAsync(id, request.NuevaFechaInicio);
            if (membresiaRenovada == null) return NotFound("No se pudo encontrar o renovar la membresía.");
            
            return Ok(membresiaRenovada);
        }
    }
}