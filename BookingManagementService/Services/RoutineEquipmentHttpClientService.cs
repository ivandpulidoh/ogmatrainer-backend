using BookingManagementService.Interfaces;
using BookingManagementService.Models.External;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BookingManagementService.Services;

public class RoutineEquipmentHttpClientService : IRoutineEquipmentServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _routineEquipmentServiceBaseUrl;
    private readonly ILogger<RoutineEquipmentHttpClientService> _logger;

    public RoutineEquipmentHttpClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RoutineEquipmentHttpClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _routineEquipmentServiceBaseUrl = configuration["RoutineEquipmentService:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("RoutineEquipmentService:BaseUrl is not configured.");
    }

    public async Task<ExternalRoutineDto?> GetRoutineByIdAsync(int routineId)
    {
        var client = _httpClientFactory.CreateClient("RoutineEquipmentServiceClient");
        string requestUrl = $"{_routineEquipmentServiceBaseUrl}/api/Routines/{routineId}";
        _logger.LogInformation("Fetching routine {RoutineId} from {Url}", routineId, requestUrl);
        try
        {
            return await client.GetFromJsonAsync<ExternalRoutineDto>(requestUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching routine {RoutineId}. Status: {StatusCode}", routineId, ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching routine {RoutineId}.", routineId);
            return null;
        }
    }

    public async Task<ExternalExerciseDto?> GetExerciseByIdAsync(int exerciseId)
    {
        var client = _httpClientFactory.CreateClient("RoutineEquipmentServiceClient");
        string requestUrl = $"{_routineEquipmentServiceBaseUrl}/api/Exercises/{exerciseId}";
        _logger.LogInformation("Fetching exercise {ExerciseId} from {Url}", exerciseId, requestUrl);
        try
        {
            return await client.GetFromJsonAsync<ExternalExerciseDto>(requestUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching exercise {ExerciseId}. Status: {StatusCode}", exerciseId, ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exercise {ExerciseId}.", exerciseId);
            return null;
        }
    }

     public async Task<ExternalRutinaDiaEjercicioDto?> GetRutinaDiaEjercicioByIdAsync(int idRutinaDiaEjercicio)
    {
        var client = _httpClientFactory.CreateClient("RoutineEquipmentServiceClient");
        // IMPORTANT: This path needs to match the new endpoint you will create in RoutineEquipmentService
        string requestUrl = $"{_routineEquipmentServiceBaseUrl}/api/Routines/day-exercise/{idRutinaDiaEjercicio}";
        _logger.LogInformation("Fetching RutinaDiaEjercicio {IdRutinaDiaEjercicio} from {Url}", idRutinaDiaEjercicio, requestUrl);

        try
        {
            return await client.GetFromJsonAsync<ExternalRutinaDiaEjercicioDto>(requestUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching RutinaDiaEjercicio {Id}. Status: {StatusCode}", idRutinaDiaEjercicio, ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching RutinaDiaEjercicio {Id}.", idRutinaDiaEjercicio);
            return null;
        }
    }
}