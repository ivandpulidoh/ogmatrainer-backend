using BookingManagementService.Models;
using BookingManagementService.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BookingManagementService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger; // Add logging

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    // POST api/bookings/machines
    [HttpPost("machines")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateMachineReservation([FromBody] CreateMachineReservationRequest request)
    {
        if (!ModelState.IsValid) // Use built-in validation
        {
            return BadRequest(ModelState);
        }

        var (success, reservationId, errorMessage) = await _bookingService.CreateMachineReservationAsync(request);

        if (success && reservationId.HasValue)
        {
            // Return 201 Created with location header (optional but good practice)
            // var location = Url.Action(nameof(GetMachineReservationById), new { id = reservationId.Value }); // Need a GetById method
             return Created($"/api/bookings/machines/{reservationId.Value}", new { reservationId = reservationId.Value });
        }
        else if (!string.IsNullOrEmpty(errorMessage) && errorMessage.Contains("already booked"))
        {
             return Conflict(new { message = errorMessage });
        }
        else
        {
             return BadRequest(new { message = errorMessage ?? "Failed to create reservation." });
        }
    }

    // POST api/bookings/trainers
    [HttpPost("trainers")]
     [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTrainerReservation([FromBody] CreateTrainerReservationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, reservationId, errorMessage) = await _bookingService.CreateTrainerReservationAsync(request);

         if (success && reservationId.HasValue)
        {
             return Created($"/api/bookings/trainers/{reservationId.Value}", new { reservationId = reservationId.Value });
        }
        else if (!string.IsNullOrEmpty(errorMessage) && (errorMessage.Contains("already booked") || errorMessage.Contains("not available")))
        {
             return Conflict(new { message = errorMessage });
        }
        else
        {
             return BadRequest(new { message = errorMessage ?? "Failed to create reservation." });
        }
    }


    // POST api/bookings/classes/{classId}/register
    [HttpPost("classes/{classId:int}/register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterInClass(int classId, [FromBody] ClassRegistrationRequest request)
    {
         if (!ModelState.IsValid || request.IdUsuario <= 0)
        {
            return BadRequest("Invalid user ID provided.");
        }

        var (success, registrationId, errorMessage) = await _bookingService.RegisterForClassAsync(classId, request.IdUsuario);

         if (success && registrationId.HasValue)
        {
             // Maybe return registration details or just 201
             return Created($"/api/bookings/classes/registrations/{registrationId.Value}", new { registrationId = registrationId.Value });
        }
        else if (!string.IsNullOrEmpty(errorMessage) && errorMessage.Contains("not found"))
        {
            return NotFound(new { message = errorMessage });
        }
         else if (!string.IsNullOrEmpty(errorMessage) && (errorMessage.Contains("full") || errorMessage.Contains("already registered")))
        {
             return Conflict(new { message = errorMessage });
        }
        else
        {
             return BadRequest(new { message = errorMessage ?? "Failed to register for class." });
        }
    }


    // GET api/bookings/user/{userId}/day/{date} (e.g., /api/bookings/user/123/day/2024-07-28)
    [HttpGet("user/{userId:int}/day/{date}")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserBookingsForDay(int userId, string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
        {
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        }

        var bookings = await _bookingService.GetUserBookingsForDayAsync(userId, parsedDate);
        return Ok(bookings);
    }


    // GET api/bookings/day/{date} (e.g., /api/bookings/day/2024-07-28)
    [HttpGet("day/{date}")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllBookingsForDay(string date)
    {
          if (!DateOnly.TryParse(date, out var parsedDate))
        {
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        }

         var bookings = await _bookingService.GetAllBookingsForDayAsync(parsedDate);
        return Ok(bookings);
    }

     // Add other endpoints as needed (e.g., GET specific reservation, DELETE reservation)
}