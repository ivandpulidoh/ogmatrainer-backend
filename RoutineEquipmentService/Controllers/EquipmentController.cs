using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Dtos;
using System.Security.Claims; // For getting user ID

namespace RoutineEquipmentService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Secure all endpoints in this controller
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;
    private readonly ILogger<EquipmentController> _logger;

    public EquipmentController(IEquipmentService equipmentService, ILogger<EquipmentController> logger)
    {
        _equipmentService = equipmentService;
        _logger = logger;
    }

    // POST api/equipment/machines
    [HttpPost("machines")]
    [ProducesResponseType(typeof(MaquinaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMachine([FromBody] CreateMaquinaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (machine, errorMessage) = await _equipmentService.CreateMachineAsync(request);
        if (machine != null)
        {
            return CreatedAtAction(nameof(GetMachineById), new { machineId = machine.IdMaquina }, machine);
        }
        return BadRequest(new ProblemDetails { Title = "Creation Failed", Detail = errorMessage });
    }

    // GET api/equipment/machines/{machineId}
    [HttpGet("machines/{machineId:int}")]
    [ProducesResponseType(typeof(MaquinaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaquinaResponse>> GetMachineById(int machineId)
    {
        var machine = await _equipmentService.GetMachineByIdAsync(machineId);
        if (machine == null) return NotFound();
        return Ok(machine);
    }

    // GET api/equipment/machines
    [HttpGet("machines")]
    [ProducesResponseType(typeof(IEnumerable<MaquinaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MaquinaResponse>>> GetAllMachines()
    {
        var machines = await _equipmentService.GetAllMachinesAsync();
        return Ok(machines);
    }

    // PUT api/equipment/machines/{machineId}
    [HttpPut("machines/{machineId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMachine(int machineId, [FromBody] UpdateMaquinaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var (success, errorMessage) = await _equipmentService.UpdateMachineAsync(machineId, request);
        if (success) return NoContent();
        if (errorMessage != null && errorMessage.Contains("not found")) return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
        return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = errorMessage });
    }


    // DELETE api/equipment/machines/{machineId}
    [HttpDelete("machines/{machineId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // For FK issues
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMachine(int machineId)
    {
        var (success, errorMessage) = await _equipmentService.DeleteMachineAsync(machineId);
        if (success) return NoContent();
        if (errorMessage != null && errorMessage.Contains("not found")) return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
        return BadRequest(new ProblemDetails { Title = "Deletion Failed", Detail = errorMessage }); // Could be 409 Conflict for FK
    }
}