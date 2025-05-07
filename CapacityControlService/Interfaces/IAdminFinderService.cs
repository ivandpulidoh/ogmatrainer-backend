namespace CapacityControlService.Interfaces;
public interface IAdminFinderService
{
    /// <summary>
    /// Gets the user IDs of administrators associated with a specific gym,
    /// potentially including global administrators.
    /// </summary>
    /// <param name="gymId">The ID of the gym.</param>
    /// <returns>A collection of unique administrator user IDs.</returns>
    Task<IEnumerable<int>> GetAdminIdsForGymAsync(int gymId);
}