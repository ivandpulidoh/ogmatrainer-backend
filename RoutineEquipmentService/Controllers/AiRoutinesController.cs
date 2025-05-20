using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoutineEquipmentService.Dtos;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoutineEquipmentService.Controllers;

[Route("api/ai-routines")]
[ApiController]
public class AiRoutinesController : ControllerBase
{
    private readonly IAiRoutineGeneratorService _aiRoutineGenerator;
    private readonly IRoutineService _routineService;
    private readonly ILogger<AiRoutinesController> _logger;

    public AiRoutinesController(
        IAiRoutineGeneratorService aiRoutineGenerator,
        IRoutineService routineService,
        ILogger<AiRoutinesController> logger)
    {
        _aiRoutineGenerator = aiRoutineGenerator;
        _routineService = routineService;
        _logger = logger;
    }

    private int GetRequiredCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            _logger.LogWarning("User ID claim not found or invalid for AI routine generation.");
            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        }
        return userId;
    }

    // POST api/ai-routines/generate-personalized
    [HttpPost("generate-personalized")]
    [ProducesResponseType(typeof(RutinaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GeneratePersonalizedRoutine([FromBody] UserProfileDto userProfile)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
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

        var (generatedRoutineRequest, aiErrorMessage) = await _aiRoutineGenerator.GenerateRoutineAsync(userProfile, creatorUserId);

        if (generatedRoutineRequest == null)
        {
            _logger.LogError("AI Routine generation failed: {ErrorMessage}", aiErrorMessage);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Title = "AI Routine Generation Failed", Detail = aiErrorMessage ?? "Could not generate routine from AI." });
        }

        
        _logger.LogInformation("AI generated routine request successfully. Attempting to save routine: {RoutineName}", generatedRoutineRequest.NombreRutina);
        var (savedRoutine, saveErrorMessage) = await _routineService.CreateRoutineAsync(generatedRoutineRequest, creatorUserId);

        if (savedRoutine != null)
        {
            _logger.LogInformation("AI generated routine '{RoutineName}' (ID: {RutinaId}) saved successfully.", savedRoutine.NombreRutina, savedRoutine.IdRutina);            
            return CreatedAtAction(nameof(RoutinesController.GetRoutineById), "Routines", new { rutinaId = savedRoutine.IdRutina }, savedRoutine);
        }
        else
        {
            _logger.LogError("Failed to save AI generated routine. AI output was valid, but saving failed: {ErrorMessage}", saveErrorMessage);            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Title = "Failed to Save AI Routine", Detail = saveErrorMessage ?? "Could not save the generated routine." });
        }
    }
}