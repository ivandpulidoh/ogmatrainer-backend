using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization; // Uncomment if using Auth

namespace CapacityControlService.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize] // Apply Auth globally if needed
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    // POST api/attendance/checkin
    [HttpPost("checkin")]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)] // Conflict for capacity
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (response, errorMessage, capacityReached) = await _attendanceService.CheckInAsync(request);

        if (response != null)
        {
            // Use CreatedAtAction if you have a GetCheckInById endpoint
            return Created($"/api/attendance/checkins/{response.CheckInId}", response);
        }

        if (capacityReached)
        {
             return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Capacity Reached", Detail = errorMessage });
        }

        return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Check-in Failed", Detail = errorMessage });
    }

    // POST api/attendance/checkout (Using POST for simplicity, PUT with CheckInId is also valid RESTfully)
    [HttpPost("checkout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)] // If no active check-in found
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
         if (!ModelState.IsValid) return BadRequest(ModelState);

         var (success, errorMessage) = await _attendanceService.CheckOutAsync(request);

         if (success)
         {
             return NoContent();
         }

         // Distinguish between Not Found and other errors if possible
         if (errorMessage != null && errorMessage.Contains("No active check-in found"))
         {
             return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Not Found", Detail = errorMessage });
         }

         return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Check-out Failed", Detail = errorMessage });
    }
}