using System.Text.Json.Serialization;
namespace RoutineEquipmentService.Models.Ai;
public class GeminiPromptPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;
}