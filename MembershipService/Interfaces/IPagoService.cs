using MembershipService.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MembershipService.Interfaces
{
    public interface IPagoService
    {
        Task<PagoDto?> CreatePagoAsync(CreatePagoDto createPagoDto, int idUsuarioSolicitante);
        Task<IEnumerable<PagoDto>> GetAllPagosAsync(int pageNumber, int pageSize);
        Task<IEnumerable<PagoDto>> GetPagosByUsuarioAsync(int idUsuario, int pageNumber, int pageSize);
        Task<PagoDto?> GetPagoByIdAsync(int idPago);
        Task<bool> UpdatePagoStatusAsync(int idPago, UpdatePagoStatusDto updateStatusDto);
    }
}