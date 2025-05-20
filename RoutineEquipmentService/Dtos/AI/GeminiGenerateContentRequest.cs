using System.Text.Json.Serialization;
namespace RoutineEquipmentService.Models.Ai;
public class GeminiGenerateContentRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = new();

    [JsonPropertyName("generationConfig")]
    public GeminiGenerationConfig GenerationConfig { get; set; } = new();
}