namespace CapacityControlService.Dtos;
public class QrCodeData
{
    public Guid QrId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime GeneratedAt { get; set; }
}