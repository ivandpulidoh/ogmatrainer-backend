using RoutineEquipmentService.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Web;

namespace RoutineEquipmentService.Services;

public class ExternalQrCodeHttpService : IExternalQrCodeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalQrCodeHttpService> _logger;

    public ExternalQrCodeHttpService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ExternalQrCodeHttpService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]?> GetQrCodeBytesAsync(string name, string? description)
    {
        var qrServiceBaseUrl = _configuration["QrCodeService:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrEmpty(qrServiceBaseUrl))
        {
            _logger.LogError("QR Code Service BaseUrl is not configured.");
            return null;
        }

        var client = _httpClientFactory.CreateClient("ExternalQrClient"); // Can be a named client or default

        // Construct the URL with query parameters
        var builder = new UriBuilder($"{qrServiceBaseUrl}/api/QrCode/generate"); // Adjust path as needed
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["name"] = name;
        if (!string.IsNullOrEmpty(description))
        {
            query["description"] = description;
        }
        builder.Query = query.ToString();
        string requestUrl = builder.ToString();

        _logger.LogInformation("Requesting QR code from: {RequestUrl}", requestUrl);

        try
        {
            HttpResponseMessage response = await client.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentType?.MediaType == "image/png")
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                _logger.LogWarning("QR Code service returned success but content type was not image/png. Type: {ContentType}", response.Content.Headers.ContentType);
                return null;
            }
            else
            {
                _logger.LogError("Failed to get QR code from external service. Status: {StatusCode}, URL: {Url}, Body: {Body}",
                    response.StatusCode, requestUrl, await response.Content.ReadAsStringAsync());
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling external QR code service at {Url}", requestUrl);
            return null;
        }
    }
}