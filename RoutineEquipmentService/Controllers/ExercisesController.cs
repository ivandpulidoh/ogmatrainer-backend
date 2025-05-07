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
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseService _exerciseService;
    private readonly ILogger<ExercisesController> _logger;

    public ExercisesController(IExerciseService exerciseService, ILogger<ExercisesController> logger)
    {
        _exerciseService = exerciseService;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        if (int.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        return null;
    }

    // POST api/exercises
    [HttpPost]
    [ProducesResponseType(typeof(EjercicioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateExercise([FromBody] CreateEjercicioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var creatorUserId = GetCurrentUserId();        

        var (exercise, errorMessage) = await _exerciseService.CreateExerciseAsync(request, creatorUserId);

        if (exercise != null)
        {
            return CreatedAtAction(nameof(GetExerciseById), new { exerciseId = exercise.IdEjercicio }, exercise);
        }
        if (errorMessage != null && errorMessage.Contains("already exists"))
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = errorMessage, Status = StatusCodes.Status409Conflict });
        }
        return BadRequest(new ProblemDetails { Title = "Creation Failed", Detail = errorMessage });
    }

    // GET api/exercises/{exerciseId}
    [HttpGet("{exerciseId:int}")]
    [ProducesResponseType(typeof(EjercicioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EjercicioResponse>> GetExerciseById(int exerciseId)
    {
        var exercise = await _exerciseService.GetExerciseByIdAsync(exerciseId);
        if (exercise == null) return NotFound();
        return Ok(exercise);
    }

    // GET api/exercises
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EjercicioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EjercicioResponse>>> GetAllExercises()
    {
        var exercises = await _exerciseService.GetAllExercisesAsync();
        return Ok(exercises);
    }

    // PUT api/exercises/{exerciseId}
    [HttpPut("{exerciseId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)] // For name conflict
    public async Task<IActionResult> UpdateExercise(int exerciseId, [FromBody] UpdateEjercicioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updaterUserId = GetCurrentUserId();
        var (success, errorMessage) = await _exerciseService.UpdateExerciseAsync(exerciseId, request, updaterUserId);

        if (success) return NoContent();
        if (errorMessage != null && errorMessage.Contains("not found")) return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
        if (errorMessage != null && errorMessage.Contains("already exists"))
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = errorMessage, Status = StatusCodes.Status409Conflict });
        }
        return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = errorMessage });
    }

    // DELETE api/exercises/{exerciseId}
    [HttpDelete("{exerciseId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // For FK issues
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExercise(int exerciseId)
    {
        var deleterUserId = GetCurrentUserId();
        var (success, errorMessage) = await _exerciseService.DeleteExerciseAsync(exerciseId, deleterUserId);

        if (success) return NoContent();
        if (errorMessage != null && errorMessage.Contains("not found")) return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
        // Could be 409 Conflict for FK issues, but BadRequest is a general fallback
        return BadRequest(new ProblemDetails { Title = "Deletion Failed", Detail = errorMessage });
    }
}