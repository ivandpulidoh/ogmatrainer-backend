using BookingManagementService.Models;

namespace BookingManagementService.Services;

public enum CancellationResult
{
    Success,
    NotFound,
    Forbidden,
    Conflict
}

public interface IBookingService
{
    Task<(bool Success, int? ReservationId, string? ErrorMessage)> CreateMachineReservationAsync(CreateMachineReservationRequest request);
    Task<(bool Success, int? ReservationId, string? ErrorMessage)> CreateTrainerReservationAsync(CreateTrainerReservationRequest request);
    Task<(bool Success, int? RegistrationId, string? ErrorMessage)> RegisterForClassAsync(int classId, int userId);
    // Add methods for cancellation if needed
    // Task<bool> CancelMachineReservationAsync(int reservationId, int userId);

    Task<IEnumerable<BookingDto>> GetUserBookingsForDayAsync(int userId, DateOnly date);
    Task<IEnumerable<BookingDto>> GetAllBookingsForDayAsync(DateOnly date);

    // Method to update attendance (could be internal or exposed if manual update needed)
    Task UpdateMachineAttendanceAsync(int reservationId, bool attended);
    Task UpdateTrainerAttendanceAsync(int reservationId, bool? clientAttended, bool? trainerAttended);
    Task UpdateClassAttendanceAsync(int registrationId, bool attended);

    Task<CancellationResult> CancelMachineReservationAsync(int reservationId, int requestingUserId);
    Task<CancellationResult> CancelTrainerReservationAsync(int reservationId, int requestingUserId, bool isAdmin);
    Task<CancellationResult> CancelClassRegistrationAsync(int registrationId, int requestingUserId, bool isAdmin);
}