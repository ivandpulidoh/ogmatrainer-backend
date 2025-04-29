namespace NotificationService.Dtos
{
    /// <summary>
    /// DTO representing a notification resource returned by the API.
    /// </summary>
    public record NotificationDto(
        Guid Id,         // The unique identifier (UUID)
        int IdUsuario,   // The associated user ID
        string Tipo,     // Notification type
        DateTime Fecha,  // Date and time of creation
        string Nombre,   // Notification title/name
        string? Descripcion // Optional description
    );
}