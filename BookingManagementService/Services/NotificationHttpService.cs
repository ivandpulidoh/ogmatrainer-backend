using BookingManagementService.Entities;
using BookingManagementService.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json; // Requires System.Net.Http.Json package if not implicitly included
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

    public async Task SendMissedReservationNotificationAsync(Usuario user, ReservaMaquina reservation)
    {
        var client = _httpClientFactory.CreateClient("NotificationServiceClient"); // Use named client

        var notification = new NotificationRequest
        {
            IdUsuario = user.IdUsuario,
            Tipo = "ReservationMissed",
            Nombre = "Reservación Perdida",
            Descripcion = $"No asististe a tu reservación de la máquina '{reservation.MaquinaEjercicio?.Nombre ?? "Desconocida"}' programada para {reservation.FechaHoraInicio.ToLocalTime():g}. La reservación ha sido marcada como no asistida."
             // ToDo: Include machine name requires loading it first if not already loaded
        };

         try
         {
            var response = await client.PostAsJsonAsync("/api/Notifications", notification); // Adjust API path if needed

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent missed reservation notification for user {UserId}, reservation {ReservationId}", user.IdUsuario, reservation.IdReservaMaquina);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send notification for user {UserId}. Status: {StatusCode}, Response: {Response}", user.IdUsuario, response.StatusCode, errorContent);
            }
        }
        catch (HttpRequestException ex)
        {
             _logger.LogError(ex, "HTTP request error sending notification for user {UserId}", user.IdUsuario);
        }
         catch (Exception ex) // Catch other potential exceptions
         {
             _logger.LogError(ex, "Error sending notification for user {UserId}", user.IdUsuario);
         }
    }
}