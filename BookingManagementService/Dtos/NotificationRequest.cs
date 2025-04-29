using System.Text.Json.Serialization;

namespace BookingManagementService.Models;

public class NotificationRequest
{
    [JsonPropertyName("idUsuario")] // Match the exact JSON property name
    public int IdUsuario { get; set; }

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = null!;

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = null!;

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = null!;
}