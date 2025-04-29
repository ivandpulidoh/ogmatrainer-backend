using UserManagementService.Dtos;

namespace UserManagementService.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Gets a user by their ID (only active users).
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The user details or null if not found or inactive.</returns>
        Task<UserReadDto?> GetUserByIdAsync(int id);

        /// <summary>
        /// Gets all active users.
        /// </summary>
        /// <returns>A collection of active users.</returns>
        Task<IEnumerable<UserReadDto>> GetAllUsersAsync();

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="userDto">Data for the new user.</param>
        /// <returns>The created user details or null if creation failed (e.g., email exists).</returns>
        Task<UserReadDto?> CreateUserAsync(UserCreateDto userDto);

        /// <summary>
        /// Updates an existing user's profile information (excluding password).
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="userDto">The updated user data.</param>
        /// <returns>True if the update was successful, false otherwise (e.g., user not found).</returns>
        Task<bool> UpdateUserAsync(int id, UserUpdateDto userDto);

        /// <summary>
        /// Updates a user's password.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="newPassword">The new plain text password.</param>
        /// <returns>True if the password update was successful, false otherwise.</returns>
        Task<bool> UpdateUserPasswordAsync(int id, string newPassword);


        /// <summary>
        /// Deletes a user (soft delete by setting Active = false).
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>True if the user was successfully marked as inactive, false otherwise.</returns>
        Task<bool> DeleteUserAsync(int id);

        // Potentially add methods for assigning/removing roles explicitly if needed later
        // Task<bool> AssignRoleToUserAsync(int userId, string roleName);
        // Task<bool> RemoveRoleFromUserAsync(int userId, string roleName);
    }
}