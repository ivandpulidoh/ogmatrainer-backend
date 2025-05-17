using MembershipService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MembershipService.Interfaces
{
    public interface ITipoMembresiaService
    {
        Task<IEnumerable<TipoMembresiaDto>> GetAllAsync();
        Task<TipoMembresiaDto?> GetByIdAsync(int id);
        Task<TipoMembresiaDto?> CreateAsync(CreateTipoMembresiaDto createDto);
        Task<bool> UpdateAsync(int id, UpdateTipoMembresiaDto updateDto);
        Task<bool> DeleteAsync(int id); // Podría ser un soft delete (cambiar Activo a false)
    }
}