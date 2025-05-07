using System.ComponentModel.DataAnnotations;
namespace CapacityControlService.Dtos;
public class CheckInRequest
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public int GymId { get; set; }
    // Optional: Add QR data if needed for validation
    // public string? QrCodeData { get; set; }
}