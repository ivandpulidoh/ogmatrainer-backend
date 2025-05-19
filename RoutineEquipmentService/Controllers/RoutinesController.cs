using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Dtos;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Secure all endpoints
public class RoutinesController : ControllerBase
{
    private readonly IRoutineService _routineService;
    private readonly ILogger<RoutinesController> _logger;

    public RoutinesController(IRoutineService routineService, ILogger<RoutinesController> logger)
    {
        _routineService = routineService;
        _logger = logger;
    }

    private int GetRequiredCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            _logger.LogWarning("User ID claim not found or invalid.");
            throw new UnauthorizedAccessException("User ID claim is missing, invalid, or user is not properly authenticated.");
        }
        return userId;
    }

    // POST api/routines
    [HttpPost]
    [ProducesResponseType(typeof(RutinaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateRoutine([FromBody] CreateRutinaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int creatorUserId;
        try
        {
            creatorUserId = GetRequiredCurrentUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message, Status = StatusCodes.Status401Unauthorized });
        }


        var (routine, errorMessage) = await _routineService.CreateRoutineAsync(request, creatorUserId);

        if (routine != null)
        {
            return CreatedAtAction(nameof(GetRoutineById), new { rutinaId = routine.IdRutina }, routine);
        }
        return BadRequest(new ProblemDetails { Title = "Creation Failed", Detail = errorMessage });
    }

    // GET api/routines/{rutinaId}
    [HttpGet("{rutinaId:int}")]
    [ProducesResponseType(typeof(RutinaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RutinaResponse>> GetRoutineById(int rutinaId)
    {
        var routine = await _routineService.GetRoutineByIdAsync(rutinaId);
        if (routine == null) return NotFound();
        return Ok(routine);
    }

    // GET api/routines
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RutinaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RutinaResponse>>> GetAllRoutines()
    {
        var routines = await _routineService.GetAllRoutinesAsync();
        return Ok(routines);
    }

    // PUT api/routines/{rutinaId}
    [HttpPut("{rutinaId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)] // For ownership issues
    public async Task<IActionResult> UpdateRoutine(int rutinaId, [FromBody] UpdateRutinaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int updaterUserId;
        try
        {
            updaterUserId = GetRequiredCurrentUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message, Status = StatusCodes.Status401Unauthorized });
        }

        var (success, errorMessage) = await _routineService.UpdateRoutineAsync(rutinaId, request, updaterUserId);

        if (success) return NoContent();

        if (errorMessage != null)
        {
            if (errorMessage.Contains("not found"))
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
            if (errorMessage.Contains("not authorized"))
                return Forbid(); // Use 403 Forbid if not authorized
        }
        return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = errorMessage });
    }

    // DELETE api/routines/{rutinaId}
    [HttpDelete("{rutinaId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // For FK issues
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)] // For ownership issues
    public async Task<IActionResult> DeleteRoutine(int rutinaId)
    {
        int deleterUserId;
        try
        {
            deleterUserId = GetRequiredCurrentUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message, Status = StatusCodes.Status401Unauthorized });
        }

        var (success, errorMessage) = await _routineService.DeleteRoutineAsync(rutinaId, deleterUserId);

        if (success) return NoContent();

        if (errorMessage != null)
        {
            if (errorMessage.Contains("not found"))
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
            if (errorMessage.Contains("not authorized"))
                return Forbid();
        }
        return BadRequest(new ProblemDetails { Title = "Deletion Failed", Detail = errorMessage });
    }

    // GET api/routines/{rutinaId}/required-machines
    [HttpGet("{rutinaId:int}/required-machines")]
    [ProducesResponseType(typeof(IEnumerable<MaquinaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequiredMachinesForRoutine(int rutinaId)
    {
        var (maquinas, errorMessage) = await _routineService.GetMaquinasForRutinaAsync(rutinaId);

        if (errorMessage != null)
        {
            if (errorMessage.Contains("not found"))
            {
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage, Status = StatusCodes.Status404NotFound });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Error", Detail = errorMessage, Status = StatusCodes.Status500InternalServerError });
        }

        return Ok(maquinas);
    }

    // GET api/Routines/day-exercise/{idRutinaDiaEjercicio}
    [HttpGet("day-exercise/{idRutinaDiaEjercicio:int}")]
    [ProducesResponseType(typeof(RutinaDiaEjercicioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RutinaDiaEjercicioResponse>> GetRutinaDiaEjercicioById(int idRutinaDiaEjercicio)
    {
        var result = await _routineService.GetRutinaDiaEjercicioByIdAsync(idRutinaDiaEjercicio);
        if (result == null)
        {
            return NotFound($"RutinaDiaEjercicio with ID {idRutinaDiaEjercicio} not found.");
        }
        return Ok(result);
    }
}