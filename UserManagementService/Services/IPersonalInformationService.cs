using UserManagementService.Dtos;
using System.Threading.Tasks; // Added for Task

namespace UserManagementService.Services
{
    public interface IPersonalInformationService
    {
        /// <summary>
        /// Gets the personal information for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The personal information DTO or null if not found.</returns>
        Task<PersonalInformationDto?> GetPersonalInformationAsync(int userId);

        /// <summary>
        /// Creates or updates the personal information for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="piDto">The personal information data.</param>
        /// <returns>True if the operation was successful, false otherwise (e.g., user not found).</returns>
        Task<bool> UpsertPersonalInformationAsync(int userId, PersonalInformationDto piDto);

         // Optional: Add a dedicated Delete method if needed, otherwise rely on User deletion cascade.
         // Task<bool> DeletePersonalInformationAsync(int userId);
    }
}
