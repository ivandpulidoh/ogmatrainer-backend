using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization; // Uncomment if using Auth

namespace CapacityControlService.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize] // Decide who can view capacity
public class CapacityController : ControllerBase
{
    private readonly ICapacityService _capacityService;
    private readonly ILogger<CapacityController> _logger;

    public CapacityController(ICapacityService capacityService, ILogger<CapacityController> logger)
    {
        _capacityService = capacityService;
        _logger = logger;
    }

    // GET api/capacity/gym/{gymId}/history?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD
    [HttpGet("gym/{gymId:int}/history")]
    [ProducesResponseType(typeof(IEnumerable<HistoricalCapacityPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    // [Authorize(Roles="Admin...")] // Likely restrict history access
    public async Task<IActionResult> GetGymHistoricalCapacity(int gymId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
         // Basic validation
         if (startDate == default || endDate == default || startDate > endDate)
         {
             return BadRequest(new ProblemDetails { Status=StatusCodes.Status400BadRequest, Title="Invalid Date Range", Detail="Please provide valid startDate and endDate." });
         }
         // Add range limit? e.g., endDate - startDate <= 30 days?

        var data = await _capacityService.GetHistoricalCapacityAsync(gymId, startDate, endDate);
        return Ok(data);
    }
}