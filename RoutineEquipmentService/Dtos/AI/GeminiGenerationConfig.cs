using System.Text.Json.Serialization;
namespace RoutineEquipmentService.Models.Ai;
public class GeminiGenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0;
    
}