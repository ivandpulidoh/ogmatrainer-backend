using System.Text.Json.Serialization;
namespace RoutineEquipmentService.Models.Ai;
public class GeminiGenerateContentResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate> Candidates { get; set; } = new();    
}