using MembershipService.DTOs;
using MembershipService.Interfaces;
using MembershipService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MembershipService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;
        private readonly ILogger<PagosController> _logger;

        public PagosController(IPagoService pagoService, ILogger<PagosController> logger)
        {
            _pagoService = pagoService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new InvalidOperationException("No se pudo obtener el ID de usuario del token.");
            }
            return userId;
        }

        [HttpPost]
        public async Task<ActionResult<PagoDto>> CreatePago([FromBody] CreatePagoDto createPagoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {                
                int solicitanteId = GetCurrentUserId();
                if (createPagoDto.IdUsuario != solicitanteId && !User.IsInRole("Administrador"))
                {
                    _logger.LogWarning("Usuario {SolicitanteId} intentando crear pago para usuario {TargetUserId} sin permisos.", solicitanteId, createPagoDto.IdUsuario);
                    return Forbid("No tiene permisos para crear un pago para otro usuario.");
                }


                var pagoDto = await _pagoService.CreatePagoAsync(createPagoDto, solicitanteId);
                if (pagoDto == null)
                {
                    return BadRequest(new { message = "No se pudo procesar el pago. Verifique los datos de la membresía." });
                }
                return CreatedAtAction(nameof(GetPagoById), new { idPago = pagoDto.IdPago }, pagoDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argumento inválido al crear pago: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al crear pago: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear pago.");
                return StatusCode(500, new { message = "Ocurrió un error interno al procesar el pago." });
            }
        }

        [HttpGet]        
        public async Task<ActionResult<IEnumerable<PagoDto>>> GetAllPagos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagos = await _pagoService.GetAllPagosAsync(pageNumber, pageSize);
            return Ok(pagos);
        }

        [HttpGet("mis-pagos")]
        public async Task<ActionResult<IEnumerable<PagoDto>>> GetMisPagos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var pagos = await _pagoService.GetPagosByUsuarioAsync(userId, pageNumber, pageSize);
            return Ok(pagos);
        }

        [HttpGet("usuario/{idUsuario}")]        
        public async Task<ActionResult<IEnumerable<PagoDto>>> GetPagosPorUsuario(int idUsuario, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagos = await _pagoService.GetPagosByUsuarioAsync(idUsuario, pageNumber, pageSize);
            return Ok(pagos);
        }


        [HttpGet("{idPago}")]
        public async Task<ActionResult<PagoDto>> GetPagoById(int idPago)
        {
            var pago = await _pagoService.GetPagoByIdAsync(idPago);
            if (pago == null)
            {
                return NotFound();
            }                        

            return Ok(pago);
        }

        [HttpPut("{idPago}/estado")]         
        public async Task<IActionResult> UpdatePagoStatus(int idPago, [FromBody] UpdatePagoStatusDto updateStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _pagoService.UpdatePagoStatusAsync(idPago, updateStatusDto);
                if (!success)
                {
                    return NotFound(new { message = $"Pago con ID {idPago} no encontrado." });
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argumento inválido al actualizar estado de pago: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar estado de pago {PagoId}.", idPago);
                return StatusCode(500, new { message = "Ocurrió un error interno." });
            }
        }
    }
}