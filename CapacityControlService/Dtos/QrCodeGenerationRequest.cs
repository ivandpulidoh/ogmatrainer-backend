using System.ComponentModel.DataAnnotations;
namespace CapacityControlService.Dtos;
public class QrCodeGenerationRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    [MaxLength(500)]
    public string? Description { get; set; }
}