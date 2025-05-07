using CapacityControlService.Data;
using CapacityControlService.Entities;
using CapacityControlService.Interfaces;
using CapacityControlService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CapacityControlService.Services;

public class SymptomService : ISymptomService
{
    private readonly CapacityDbContext _context;
    private readonly ILogger<SymptomService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IAttendanceService _attendanceService;
    private readonly IAdminFinderService _adminFinderService;

    public SymptomService(CapacityDbContext context,
        ILogger<SymptomService> logger,
        INotificationService notificationService,
        IAttendanceService attendanceService,
        IAdminFinderService adminFinderService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
        _attendanceService = attendanceService;
        _adminFinderService = adminFinderService;
    }

    public async Task<(SymptomFormResponse? Response, string? ErrorMessage)> SubmitFormAsync(SymptomFormRequest request)
    {
        _logger.LogInformation("Attempting to submit symptom form for User {UserId}, Checkin {CheckinId}", request.UserId, request.CheckInId);

        var checkIn = await _context.CheckIns
                                    .Include(c => c.Usuario)
                                    .Include(c => c.Gimnasio)
                                    .FirstOrDefaultAsync(c => c.IdCheckin == request.CheckInId && c.IdUsuario == request.UserId);

        if (checkIn == null)
        {
            return (null, "Valid Check-in record not found for the user.");
        }
        if (checkIn.Gimnasio == null)
        {
            _logger.LogError("CheckIn {CheckInId} is missing associated Gym data.", checkIn.IdCheckin);
            return (null, "Internal error: Missing Gym data for Check-in.");
        }
        if (checkIn.Usuario == null)
        {
            _logger.LogError("CheckIn {CheckInId} is missing associated User data.", checkIn.IdCheckin);
            return (null, "Internal error: Missing User data for Check-in.");
        }

        // Check if form already submitted for this check-in
        var existingForm = await _context.FormulariosSintomas
                                        .AnyAsync(f => f.IdCheckin == request.CheckInId);
        if (existingForm)
        {
            _logger.LogWarning("Symptom form already submitted for Checkin {CheckinId}", request.CheckInId);
            return (null, "Symptom form has already been submitted for this check-in.");
        }


        // Determine evaluation result
        string evaluationResult = (request.HasSymptoms || request.HasRecentContact) ? "Rechazado" : "Aprobado";

        var newForm = new FormularioSintomas
        {
            IdCheckin = request.CheckInId,
            IdUsuario = request.UserId,
            FechaEnvio = DateTime.UtcNow,
            TieneSintomas = request.HasSymptoms,
            TuvoContactoReciente = request.HasRecentContact,
            ResultadoEvaluacion = evaluationResult
        };

        try
        {
            _context.FormulariosSintomas.Add(newForm);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Symptom form submitted successfully for Checkin {CheckinId}. Result: {Result}", request.CheckInId, evaluationResult);

            var response = new SymptomFormResponse
            {
                FormId = newForm.IdFormulario,
                CheckInId = newForm.IdCheckin,
                UserId = newForm.IdUsuario,
                SubmissionTime = newForm.FechaEnvio,
                HasSymptoms = newForm.TieneSintomas,
                HasRecentContact = newForm.TuvoContactoReciente,
                EvaluationResult = newForm.ResultadoEvaluacion
            };

            if (evaluationResult == "Rechazado")
            {
                _logger.LogWarning("Symptom form {FormId} for User {UserId} at Gym {GymId} resulted in rejection. Initiating admin notification and user checkout.",
                newForm.IdFormulario, request.UserId, checkIn.IdGimnasio);

                // 1. Notify Admins
                var adminIds = await _adminFinderService.GetAdminIdsForGymAsync(checkIn.IdGimnasio);
                if (adminIds.Any())
                {
                    var userName = $"{checkIn.Usuario?.Nombre} {checkIn.Usuario?.Apellido}".Trim();
                    if (string.IsNullOrEmpty(userName)) userName = $"Usuario ID {checkIn.IdUsuario}";

                    foreach (var adminId in adminIds)
                    {
                        var notification = new NotificationRequest
                        {
                            IdUsuario = adminId,
                            Tipo = "SymptomFormRejected",
                            Nombre = "Alerta: Formulario de Síntomas Rechazado",
                            Descripcion = $"El usuario '{userName}' (ID: {checkIn.IdUsuario}) fue rechazado por el formulario de síntomas al intentar ingresar al gimnasio '{checkIn.Gimnasio?.Nombre ?? "Desconocido"}'. Razón: Presencia de síntomas o contacto reciente."
                        };
                        _ = _notificationService.SendNotificationAsync(notification);
                    }
                }
                else
                {
                    _logger.LogWarning("No administrators found for Gym {GymId} to notify about rejected form {FormId}.", checkIn.IdGimnasio, newForm.IdFormulario);
                }

                // 2. Automatically Check Out User
                var checkOutRequest = new CheckOutRequest
                {
                    UserId = checkIn.IdUsuario,
                    GymId = checkIn.IdGimnasio
                };
                _logger.LogInformation("Attempting automatic checkout for User {UserId} from Gym {GymId} due to rejected symptom form.", checkOutRequest.UserId, checkOutRequest.GymId);
                var (checkoutSuccess, checkoutError) = await _attendanceService.CheckOutAsync(checkOutRequest);
                if (!checkoutSuccess)
                {
                    _logger.LogError("Automatic checkout failed for User {UserId} after symptom form rejection. Error: {Error}", checkOutRequest.UserId, checkoutError);
                }
                else
                {
                    _logger.LogInformation("Automatic checkout successful for User {UserId} from Gym {GymId}.", checkOutRequest.UserId, checkOutRequest.GymId);
                }

                return (response, null);
            }
            else
            {
                return (response, null);
            }
        }
        catch (DbUpdateException ex) // Catch potential foreign key or unique constraint issues
        {
            _logger.LogError(ex, "Database error submitting symptom form for Checkin {CheckinId}", request.CheckInId);
            return (null, "Failed to save symptom form due to a database constraint.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting symptom form for Checkin {CheckinId}", request.CheckInId);
            return (null, "Failed to save symptom form.");
        }
    }

    public async Task<SymptomFormResponse?> GetFormByIdAsync(int formId)
    {
        var form = await _context.FormulariosSintomas
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.IdFormulario == formId);

        if (form == null) return null;

        return MapToResponse(form);
    }

    public async Task<SymptomFormResponse?> GetFormByCheckInIdAsync(int checkInId)
    {
        var form = await _context.FormulariosSintomas
           .AsNoTracking()
           .FirstOrDefaultAsync(f => f.IdCheckin == checkInId);

        if (form == null) return null;

        return MapToResponse(form);
    }

    private SymptomFormResponse MapToResponse(FormularioSintomas form)
    {
        return new SymptomFormResponse
        {
            FormId = form.IdFormulario,
            CheckInId = form.IdCheckin,
            UserId = form.IdUsuario,
            SubmissionTime = form.FechaEnvio,
            HasSymptoms = form.TieneSintomas,
            HasRecentContact = form.TuvoContactoReciente,
            EvaluationResult = form.ResultadoEvaluacion
        };
    }
}