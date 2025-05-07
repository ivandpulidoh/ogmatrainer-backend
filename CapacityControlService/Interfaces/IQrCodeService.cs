using CapacityControlService.Dtos;
namespace CapacityControlService.Interfaces;
public interface IQrCodeService {
    byte[] GenerateQrCode(QrCodeGenerationRequest request);
}