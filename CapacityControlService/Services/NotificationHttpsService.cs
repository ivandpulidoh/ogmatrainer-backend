// Services/NotificationHttpService.cs
using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using System.Net.Http;
using System.Net.Http.Json;

namespace CapacityControlService.Services;

public class NotificationHttpService : INotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationHttpService> _logger;
    private readonly IConfiguration _configuration;

    public NotificationHttpService(IHttpClientFactory httpClientFactory, ILogger<NotificationHttpService> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendNotificationAsync(NotificationRequest notification)
    {
         if (notification == null) return;
        var client = _httpClientFactory.CreateClient("NotificationServiceClient"); // Name from Program.cs
         var apiUrl = _configuration["NotificationService:BaseUrl"]?.TrimEnd('/') + "/api/Notifications"; // Example path

         try
         {
             var response = await client.PostAsJsonAsync(apiUrl, notification);
             if (response.IsSuccessStatusCode) {
                  _logger.LogInformation("Notification '{Type}' sent successfully to user {UserId}.", notification.Tipo, notification.IdUsuario);
             } else {
                  _logger.LogError("Failed to send notification '{Type}' to user {UserId}. Status: {Status}, Body: {Body}",
                     notification.Tipo, notification.IdUsuario, response.StatusCode, await response.Content.ReadAsStringAsync());
             }
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Error sending notification '{Type}' to user {UserId}.", notification.Tipo, notification.IdUsuario);
         }
    }
}