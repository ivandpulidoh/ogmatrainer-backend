namespace RoutineEquipmentService.Interfaces;
public interface IExternalQrCodeService {
    Task<byte[]?> GetQrCodeBytesAsync(string name, string? description);
}