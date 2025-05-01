using BookingManagementService.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BookingManagementService.Services;

public class NotificationHttpService : INotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationHttpService> _logger;

    public NotificationHttpService(IHttpClientFactory httpClientFactory, ILogger<NotificationHttpService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // Implement the generic method
    public async Task SendNotificationAsync(NotificationRequest notification)
    {
        if (notification == null)
        {
            _logger.LogWarning("Attempted to send a null notification.");
            return; // Or throw ArgumentNullException
        }

        var client = _httpClientFactory.CreateClient("NotificationServiceClient");

        try
        {
            var response = await client.PostAsJsonAsync("/api/Notifications", notification);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent notification '{NotificationType}' for user {UserId}.",
                    notification.Tipo, notification.IdUsuario);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send notification '{NotificationType}' for user {UserId}. Status: {StatusCode}, Response: {Response}",
                    notification.Tipo, notification.IdUsuario, response.StatusCode, errorContent);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error sending notification '{NotificationType}' for user {UserId}",
                 notification.Tipo, notification.IdUsuario);
        }
        catch (Exception ex) // Catch other potential exceptions
        {
            _logger.LogError(ex, "Error sending notification '{NotificationType}' for user {UserId}",
                 notification.Tipo, notification.IdUsuario);
        }
    }
}