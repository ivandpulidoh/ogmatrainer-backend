using MembershipService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MembershipService.Interfaces
{
    public interface IMembresiaService
    {
        Task<IEnumerable<MembresiaDto>> GetAllAsync();
        Task<IEnumerable<MembresiaDto>> GetByUsuarioIdAsync(int usuarioId);
        Task<MembresiaDto?> GetByIdAsync(int id);
        Task<MembresiaDto?> CreateAsync(CreateMembresiaDto createDto);
        Task<bool> UpdateAsync(int id, UpdateMembresiaDto updateDto);
        Task<bool> CancelAsync(int id);
        Task<MembresiaDto?> RenewAsync(int id, DateOnly nuevaFechaInicio);
    }
}