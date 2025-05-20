// Services/GeminiAiRoutineGeneratorService.cs
using RoutineEquipmentService.Interfaces;
using RoutineEquipmentService.Models;
using RoutineEquipmentService.Models.Ai; // For Gemini DTOs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
using RoutineEquipmentService.Dtos;

namespace RoutineEquipmentService.Services;

public class GeminiAiRoutineGeneratorService : IAiRoutineGeneratorService
{
    private readonly IExerciseService _exerciseService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiRoutineGeneratorService> _logger;
    private readonly string? _geminiApiKey;

    public GeminiAiRoutineGeneratorService(
        IExerciseService exerciseService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GeminiAiRoutineGeneratorService> logger)
    {
        _exerciseService = exerciseService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _geminiApiKey = _configuration["Gemini:ApiKey"];
    }

    public async Task<(CreateRutinaRequest? GeneratedRoutine, string? ErrorMessage)> GenerateRoutineAsync(UserProfileDto userProfile, int creatorUserId)
    {
        if (string.IsNullOrEmpty(_geminiApiKey))
        {
            _logger.LogError("Gemini API Key is not configured.");
            return (null, "AI service is not configured (missing API key).");
        }
        
        var allExercises = await _exerciseService.GetAllExercisesAsync();
        if (allExercises == null || !allExercises.Any())
        {
            _logger.LogWarning("No exercises found in the database to build the AI prompt.");
            return (null, "No exercises available to generate a routine.");
        }
        
        string exercisesJson = JsonSerializer.Serialize(allExercises, new JsonSerializerOptions { WriteIndented = true });

        
        string promptText = BuildPrompt(userProfile, exercisesJson);

        
        var geminiRequest = new GeminiGenerateContentRequest
        {
            Contents = new List<GeminiContent>
            {
                new GeminiContent { Parts = new List<GeminiPromptPart> { new GeminiPromptPart { Text = promptText } } }
            },
            GenerationConfig = new GeminiGenerationConfig { Temperature = 0.2 } // Lower temperature for more deterministic output
        };

        var httpClient = _httpClientFactory.CreateClient("GeminiApiClient");
        string geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-04-17:generateContent?key={_geminiApiKey}";        

        _logger.LogInformation("Sending request to Gemini API. Prompt length: {Length}", promptText.Length);
        
        _logger.LogDebug("Gemini Prompt: {Prompt}", promptText);


        HttpResponseMessage geminiHttpResponse;
        try
        {
            geminiHttpResponse = await httpClient.PostAsJsonAsync(geminiUrl, geminiRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API.");
            return (null, $"Error communicating with AI service: {ex.Message}");
        }

        if (!geminiHttpResponse.IsSuccessStatusCode)
        {
            string errorContent = await geminiHttpResponse.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API request failed. Status: {StatusCode}, Response: {ErrorContent}", geminiHttpResponse.StatusCode, errorContent);
            return (null, $"AI service request failed with status {geminiHttpResponse.StatusCode}. Details: {errorContent}");
        }
        
        try
        {
            var geminiResponse = await geminiHttpResponse.Content.ReadFromJsonAsync<GeminiGenerateContentResponse>();
            if (geminiResponse == null || geminiResponse.Candidates == null || !geminiResponse.Candidates.Any() ||
                geminiResponse.Candidates[0].Content?.Parts == null || !geminiResponse.Candidates[0].Content.Parts.Any())
            {
                _logger.LogWarning("Gemini API returned an empty or invalid response structure.");
                string rawResponseForDebug = await geminiHttpResponse.Content.ReadAsStringAsync();
                _logger.LogDebug("Raw Gemini Response: {RawResponse}", rawResponseForDebug);
                return (null, "AI service returned an unexpected or empty response.");
            }

            string routineJsonFromAi = geminiResponse.Candidates[0].Content!.Parts[0].Text;
            _logger.LogInformation("Received routine JSON from AI.");
            
            _logger.LogDebug("Routine JSON from AI: {Json}", routineJsonFromAi);

           
            string routineJsonFromAiCandidate = geminiResponse.Candidates[0].Content!.Parts[0].Text;
            Console.WriteLine("===== EXTRACTED ROUTINE JSON FROM AI CANDIDATE =====");
            Console.WriteLine(routineJsonFromAiCandidate);
            Console.WriteLine("====================================================");
            
            // --- >>> LIMPIAR EL MARKDOWN <<< ---
            string cleanedRoutineJson = routineJsonFromAiCandidate.Trim();

            if (cleanedRoutineJson.StartsWith("```json"))
            {
                cleanedRoutineJson = cleanedRoutineJson.Substring("```json".Length);
            }
            else if (cleanedRoutineJson.StartsWith("```"))
            {
                cleanedRoutineJson = cleanedRoutineJson.Substring("```".Length);
            }

            if (cleanedRoutineJson.EndsWith("```"))
            {
                cleanedRoutineJson = cleanedRoutineJson.Substring(0, cleanedRoutineJson.Length - "```".Length);
            }
            cleanedRoutineJson = cleanedRoutineJson.Trim();
            // --- >>> LIMPIAR EL MARKDOWN <<< ---
               
            var generatedRoutineRequest = JsonSerializer.Deserialize<CreateRutinaRequest>(cleanedRoutineJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); 

            if (generatedRoutineRequest == null)
            {
                _logger.LogError("Failed to deserialize AI response into CreateRutinaRequest. AI Output: {AiOutput}", cleanedRoutineJson);
                return (null, "AI service generated an invalid routine format.");
            }

            
            if (string.IsNullOrEmpty(generatedRoutineRequest.NombreRutina))
            {
                generatedRoutineRequest.NombreRutina = $"Rutina IA para {userProfile.ObjetivoPrincipal.Substring(0, Math.Min(userProfile.ObjetivoPrincipal.Length, 20))}";
            }
            if (string.IsNullOrEmpty(generatedRoutineRequest.Nivel))
            {
                generatedRoutineRequest.Nivel = userProfile.ExperienciaEntrenamiento;
            }


            return (generatedRoutineRequest, null);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error parsing JSON response from Gemini API.");
            string rawResponseForDebug = await geminiHttpResponse.Content.ReadAsStringAsync(); // Get raw for debugging
            _logger.LogDebug("Raw Gemini Response causing JSON error: {RawResponse}", rawResponseForDebug);
            return (null, "Error parsing AI service response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing Gemini API response.");
            return (null, "Unexpected error processing AI service response.");
        }
    }

    private string BuildPrompt(UserProfileDto userProfile, string exercisesJson)
    {
        // Serialize userProfile to JSON string for the prompt
        string userProfileJson = JsonSerializer.Serialize(userProfile, new JsonSerializerOptions { WriteIndented = true });

        // Using StringBuilder for better performance with large strings
        var sb = new StringBuilder();
        sb.AppendLine("Actúa como un entrenador personal experto. Crea una rutina de entrenamiento personalizada siguiendo los siguientes pasos:");
        sb.AppendLine("1. toma los siguientes datos del perfil del usuario:");
        sb.AppendLine(userProfileJson); // Inject user profile JSON
        sb.AppendLine();
        sb.AppendLine("2. solo puedes utilizar el siguiente listado de ejercicios para armar la rutina personalizada:");
        sb.AppendLine(exercisesJson); // Inject exercises JSON
        sb.AppendLine();
        sb.AppendLine("3. Con base en la información suministrada, responda un json object con la rutina personalizada utilizando la siguiente estructura, esto con el fin de utilizar el json que respondes como body para una api que crea rutinas, el campo series debe ser el numero de repeticiones entre comillas:");
        sb.AppendLine(@"
{
  ""nombreRutina"": ""string"",
  ""descripcion"": ""string"",
  ""nivel"": ""string"",
  ""objetivo"": ""string"",
  ""numeroDias"": 0,
  ""urlImagen"": ""string OR null"",
  ""diasEjercicios"": [
    {
      ""diaNumero"": 0,
      ""idEjercicio"": 0,
      ""ordenEnDia"": 0,
      ""series"": ""string (e.g., \""3\"")"",
      ""repeticiones"": ""string (e.g., \""8-12\"")"",
      ""descansoSegundos"": 0,
      ""notasEjercicio"": ""string OR null""
    }
  ]
}");
        sb.AppendLine();
        sb.AppendLine("4. te comparto un ejemplo de una rutina siguiendo la estructura:");
        sb.AppendLine(@"
{    
    ""nombreRutina"": ""Acondicionamiento Metabólico Avanzado"",
    ""descripcion"": ""Circuito de alta intensidad."",
    ""nivel"": ""Avanzado"",
    ""objetivo"": ""Resistencia"",    
    ""numeroDias"": 2,
    ""urlImagen"": ""https://i.ibb.co/JWVhQf9z/eliptica-grande.jpg"",
    ""diasEjercicios"": [
        {            
            ""diaNumero"": 1,
            ""idEjercicio"": 1,
            ""ordenEnDia"": 1,
            ""series"": ""3"",
            ""repeticiones"": ""12"",
            ""descansoSegundos"": 60,
            ""notasEjercicio"": ""Focus on controlled movement""
        },
        {            
            ""diaNumero"": 1,
            ""idEjercicio"": 3,
            ""ordenEnDia"": 2,
            ""series"": ""3"",
            ""repeticiones"": ""10"",
            ""descansoSegundos"": 75,
            ""notasEjercicio"": null
        },
        {            
            ""diaNumero"": 1,
            ""idEjercicio"": 5,
            ""ordenEnDia"": 3,
            ""series"": ""4"",
            ""repeticiones"": ""15"",
            ""descansoSegundos"": 45,
            ""notasEjercicio"": ""Lighter weight, focus on contraction""
        },
        {            
            ""diaNumero"": 2,
            ""idEjercicio"": 2,
            ""ordenEnDia"": 1,
            ""series"": ""4"",
            ""repeticiones"": ""8"",
            ""descansoSegundos"": 90,
            ""notasEjercicio"": ""Heavier sets""
        },
        {            
            ""diaNumero"": 2,
            ""idEjercicio"": 4,
            ""ordenEnDia"": 2,
            ""series"": ""3"",
            ""repeticiones"": ""12"",
            ""descansoSegundos"": 60,
            ""notasEjercicio"": null
        }
    ]
}");
        sb.AppendLine();
        sb.AppendLine("5. No respondas nada adicional al json object ni expliques tu respuesta. Asegúrate de que el JSON sea válido y cumpla estrictamente con la estructura solicitada, especialmente los tipos de datos (números para diaNumero, idEjercicio, ordenEnDia, descansoSegundos; strings para los demás, incluyendo series y repeticiones).");
        sb.AppendLine("Considera la disponibilidad de entrenamiento del usuario para distribuir los días de la rutina. El campo 'numeroDias' en la respuesta debe reflejar cuántos días distintos de entrenamiento has planificado. El campo 'ejercicioNombre' no debe estar en la respuesta de 'diasEjercicios', solo 'idEjercicio'.");
        sb.AppendLine("IMPORTANTE: La respuesta DEBE ser ÚNICAMENTE el objeto JSON válido, sin ningún tipo de formato Markdown, como ```json o ```, ni ningún texto explicativo antes o después del JSON.");         

        return sb.ToString();
    }    
}