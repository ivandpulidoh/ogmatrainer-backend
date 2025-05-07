using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using QRCoder; 
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;

namespace CapacityControlService.Services;

public class QrCodeService : IQrCodeService
{
     private readonly ILogger<QrCodeService> _logger;

     public QrCodeService(ILogger<QrCodeService> logger)
     {
         _logger = logger;
     }

    public byte[] GenerateQrCode(QrCodeGenerationRequest request)
    {
        var qrDataModel = new QrCodeData
        {
            QrId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            GeneratedAt = DateTime.UtcNow
        };

        // Serialize the data model to JSON (or another compact format)
        string payload = JsonSerializer.Serialize(qrDataModel);
        _logger.LogInformation("Generating QR Code with payload: {Payload}", payload);


        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q); // Quality Level Q

        // Using PngByteQRCode for direct byte array output
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeAsPngBytes = qrCode.GetGraphic(20); // Pixels per module (size)

         _logger.LogInformation("QR Code generated successfully with ID {QrId}", qrDataModel.QrId);

        return qrCodeAsPngBytes;

        // --- Alternative using Bitmap (requires System.Drawing.Common package, check compatibility) ---
        /*
        BitmapByteQRCode qrCodeBitmap = new BitmapByteQRCode(qrCodeData);
        byte[] qrCodeAsBitmapBytes = qrCodeBitmap.GetGraphic(20);
        return qrCodeAsBitmapBytes;
        */
        // --- Alternative manual Bitmap rendering (requires System.Drawing.Common) ---
        /*
         QRCode qrCodeImage = new QRCode(qrCodeData);
         Bitmap qrBitmap = qrCodeImage.GetGraphic(20); // Size factor

         using (MemoryStream stream = new MemoryStream())
         {
             qrBitmap.Save(stream, ImageFormat.Png);
             return stream.ToArray();
         }
        */
    }
}