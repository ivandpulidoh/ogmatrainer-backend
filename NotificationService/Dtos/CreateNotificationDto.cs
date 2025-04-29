using System.ComponentModel.DataAnnotations;

namespace NotificationService.Dtos
{
    /// <summary>
    /// DTO for creating a new notification.
    /// Contains data provided by the client.
    /// </summary>
    public record CreateNotificationDto(
        [Required]
        int IdUsuario,

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, ErrorMessage = "Type cannot be longer than 100 characters.")]
        string Tipo,

        [Required(AllowEmptyStrings = false)]
        [StringLength(255, ErrorMessage = "Name cannot be longer than 255 characters.")]
        string Nombre,

        string? Descripcion // Optional description
    );
}