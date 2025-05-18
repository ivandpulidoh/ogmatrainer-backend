using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoutineEquipmentService.Dtos;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Controllers;

[Route("api/espacios")] // Changed route to be more RESTful
[ApiController]
[Authorize] // Secure all endpoints
public class EspaciosController : ControllerBase
{
    private readonly IEspacioService _espacioService;
    private readonly ILogger<EspaciosController> _logger;

    public EspaciosController(IEspacioService espacioService, ILogger<EspaciosController> logger)
    {
        _espacioService = espacioService;
        _logger = logger;
    }

    // POST api/espacios
    [HttpPost]
    [ProducesResponseType(typeof(EspacioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEspacio([FromBody] CreateEspacioRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (espacio, errorMessage) = await _espacioService.CreateEspacioAsync(request);

        if (espacio != null)
        {
            return CreatedAtAction(nameof(GetEspacioById), new { espacioId = espacio.IdEspacio }, espacio);
        }

        if (errorMessage != null && errorMessage.Contains("already exists"))
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = errorMessage, Status = StatusCodes.Status409Conflict });
        }
        return BadRequest(new ProblemDetails { Title = "Creation Failed", Detail = errorMessage });
    }

    // GET api/espacios/{espacioId}
    [HttpGet("{espacioId:int}")]
    [ProducesResponseType(typeof(EspacioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EspacioResponse>> GetEspacioById(int espacioId)
    {
        var espacio = await _espacioService.GetEspacioByIdAsync(espacioId);
        if (espacio == null)
        {
            return NotFound();
        }
        return Ok(espacio);
    }

    // GET api/espacios
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EspacioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EspacioResponse>>> GetAllEspacios([FromQuery] int? gymId)
    {
        var espacios = await _espacioService.GetAllEspaciosAsync(gymId);
        return Ok(espacios);
    }

    // PUT api/espacios/{espacioId}
    [HttpPut("{espacioId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateEspacio(int espacioId, [FromBody] UpdateEspacioRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage) = await _espacioService.UpdateEspacioAsync(espacioId, request);

        if (success)
        {
            return NoContent();
        }

        if (errorMessage != null)
        {
            if (errorMessage.Contains("not found"))
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
            if (errorMessage.Contains("already exists"))
                return Conflict(new ProblemDetails { Title = "Conflict", Detail = errorMessage, Status = StatusCodes.Status409Conflict });
        }
        return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = errorMessage });
    }

    // DELETE api/espacios/{espacioId}
    [HttpDelete("{espacioId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // For FK or other issues
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEspacio(int espacioId)
    {
        var (success, errorMessage) = await _espacioService.DeleteEspacioAsync(espacioId);

        if (success)
        {
            return NoContent();
        }
        if (errorMessage != null && errorMessage.Contains("not found"))
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
        }
        // Could be 409 Conflict if in use, but BadRequest is a general fallback for deletion issues.
        return BadRequest(new ProblemDetails { Title = "Deletion Failed", Detail = errorMessage });
    }
}