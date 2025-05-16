using System.ComponentModel.DataAnnotations; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Models;
using System.Security.Claims;
using System.Collections.Generic; // For IEnumerable
using System.Threading.Tasks; // For Task

namespace RoutineEquipmentService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Secure all endpoints in this controller
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly ILogger<ClassesController> _logger;

    public ClassesController(IClassService classService, ILogger<ClassesController> logger)
    {
        _classService = classService;
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

    // POST api/classes
    [HttpPost]
    [ProducesResponseType(typeof(ClaseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateClase([FromBody] CreateClaseRequest request)
    {
        if (!ModelState.IsValid)
        {
            // Also perform custom validation if present on DTO
            var validationResults = new List<ValidationResult>();
            var isValidCustom = Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);
            if (!isValidCustom)
            {
                foreach (var validationResult in validationResults)
                {
                    ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty, validationResult.ErrorMessage ?? "Validation error");
                }
                return ValidationProblem(ModelState);
            }
        }


        int creatorUserId;
        try
        {
            creatorUserId = GetRequiredCurrentUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message, Status = StatusCodes.Status401Unauthorized });
        }

        var (clase, errorMessage) = await _classService.CreateClaseAsync(request, creatorUserId);

        if (clase != null)
        {
            return CreatedAtAction(nameof(GetClaseById), new { claseId = clase.IdClase }, clase);
        }
        return BadRequest(new ProblemDetails { Title = "Creation Failed", Detail = errorMessage });
    }

    // GET api/classes/{claseId}
    [HttpGet("{claseId:int}")]
    [ProducesResponseType(typeof(ClaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaseResponse>> GetClaseById(int claseId)
    {
        var clase = await _classService.GetClaseByIdAsync(claseId);
        if (clase == null) return NotFound();
        return Ok(clase);
    }

    // GET api/classes
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClaseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClaseResponse>>> GetAllClases(
        [FromQuery] int? gymId,
        [FromQuery] string? tipo,
        [FromQuery] bool? activa)
    {
        var clases = await _classService.GetAllClasesAsync(gymId, tipo, activa);
        return Ok(clases);
    }

    // PUT api/classes/{claseId}
    [HttpPut("{claseId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)] // For ownership issues
    public async Task<IActionResult> UpdateClase(int claseId, [FromBody] UpdateClaseRequest request)
    {
         if (!ModelState.IsValid)
        {
            var validationResults = new List<ValidationResult>();
            var isValidCustom = Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);
            if (!isValidCustom)
            {
                foreach (var validationResult in validationResults) { ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty, validationResult.ErrorMessage ?? "Validation error"); }
                return ValidationProblem(ModelState);
            }
        }

        int updaterUserId;
        try
        {
            updaterUserId = GetRequiredCurrentUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message, Status = StatusCodes.Status401Unauthorized });
        }

        var (success, errorMessage) = await _classService.UpdateClaseAsync(claseId, request, updaterUserId);

        if (success) return NoContent();

        if (errorMessage != null)
        {
            if (errorMessage.Contains("not found"))
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
            if (errorMessage.Contains("not authorized")) // Assuming service layer returns this specific message
                return Forbid();
        }
        return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = errorMessage });
    }

    // DELETE api/classes/{claseId}
    [HttpDelete("{claseId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // For FK issues
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)] // For ownership issues
    public async Task<IActionResult> DeleteClase(int claseId)
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

        var (success, errorMessage) = await _classService.DeleteClaseAsync(claseId, deleterUserId);

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
}