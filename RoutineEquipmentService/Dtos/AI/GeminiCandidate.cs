using System.Text.Json.Serialization;
namespace RoutineEquipmentService.Models.Ai;
public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
    // Add FinishReason, SafetyRatings, etc., if you need to inspect them
}