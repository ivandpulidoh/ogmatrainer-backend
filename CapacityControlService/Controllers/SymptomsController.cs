using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization; // Uncomment if using Auth

namespace CapacityControlService.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize] // Apply Auth globally if needed
public class SymptomsController : ControllerBase
{
    private readonly ISymptomService _symptomService;
    private readonly ILogger<SymptomsController> _logger;

    public SymptomsController(ISymptomService symptomService, ILogger<SymptomsController> logger)
    {
        _symptomService = symptomService;
        _logger = logger;
    }

    // POST api/symptoms/forms
    [HttpPost("forms")]
    [ProducesResponseType(typeof(SymptomFormResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)] // If already submitted
    public async Task<IActionResult> SubmitForm([FromBody] SymptomFormRequest request)
    {
         if (!ModelState.IsValid) return BadRequest(ModelState);

         var (response, errorMessage) = await _symptomService.SubmitFormAsync(request);

        if (response != null)
        {
             return Created($"/api/symptoms/forms/{response.FormId}", response);
        }

        // Check error message for specific conflicts
         if (errorMessage != null && errorMessage.Contains("already been submitted"))
         {
             return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Already Submitted", Detail = errorMessage });
         }

         return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Submission Failed", Detail = errorMessage });
    }

    // GET api/symptoms/forms/{formId}
    [HttpGet("forms/{formId:int}")]
    [ProducesResponseType(typeof(SymptomFormResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [Authorize(Roles="Admin...")] // Restrict who can view arbitrary forms
    public async Task<ActionResult<SymptomFormResponse>> GetForm(int formId)
    {
        var form = await _symptomService.GetFormByIdAsync(formId);
        if (form == null) return NotFound();
        return Ok(form);
    }

     // GET api/symptoms/forms/checkin/{checkInId}
    [HttpGet("forms/checkin/{checkInId:int}")]
    [ProducesResponseType(typeof(SymptomFormResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
     // Allow user to get their own form for their check-in, or admin
    public async Task<ActionResult<SymptomFormResponse>> GetFormByCheckIn(int checkInId)
    {
         // TODO: Add authorization check: Ensure logged-in user owns this check-in OR is admin

         var form = await _symptomService.GetFormByCheckInIdAsync(checkInId);
         if (form == null) return NotFound();
         return Ok(form);
    }
}