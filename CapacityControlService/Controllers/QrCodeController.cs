using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Authorization; // Uncomment if using Auth

namespace CapacityControlService.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize] // Decide who can generate QRs
public class QrCodeController : ControllerBase
{
    private readonly IQrCodeService _qrCodeService;

    public QrCodeController(IQrCodeService qrCodeService)
    {
        _qrCodeService = qrCodeService;
    }

    // GET api/qrcodes/generate?name=Example&description=OptionalDesc
    [HttpGet("generate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "image/png")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GenerateQrCode([FromQuery] QrCodeGenerationRequest request)
    {
        // ModelState validation handles [Required] etc. on the request DTO
        if (!ModelState.IsValid)
        {
             return ValidationProblem(ModelState);
        }
        try
        {
            byte[] qrCodeBytes = _qrCodeService.GenerateQrCode(request);
            return File(qrCodeBytes, "image/png"); // Return as PNG image bytes
        }
        catch (Exception ex)
        {
             // Log the exception
             return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate QR code.");
        }
    }
}