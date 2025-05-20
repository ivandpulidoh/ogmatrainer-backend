using System.Text.Json.Serialization;
namespace RoutineEquipmentService.Models.Ai;
public class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPromptPart> Parts { get; set; } = new();
}