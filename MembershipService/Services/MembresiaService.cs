using AutoMapper;
using MembershipService.Data;
using MembershipService.DTOs;
using MembershipService.Interfaces;
using MembershipService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipService.Services
{
    public class MembresiaService : IMembresiaService
    {
        private readonly MembershipDbContext _context;
        private readonly IMapper _mapper;

        public MembresiaService(MembershipDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MembresiaDto?> CreateAsync(CreateMembresiaDto createDto)
        {
            // Validar que el tipo de membresía existe y está activo
            var tipoMembresia = await _context.TiposMembresia
                                            .FirstOrDefaultAsync(tm => tm.IdTipoMembresia == createDto.IdTipoMembresia && tm.Activo);
            if (tipoMembresia == null)
            {
                // Podrías lanzar una excepción específica o retornar null/mensaje de error
                throw new ArgumentException("Tipo de membresía no válido o inactivo.");
            }

            // Aquí podrías verificar si el IdUsuario y IdGimnasioPrincipal son válidos
            // llamando a otros servicios o verificando en una base de datos compartida si aplica.
            // Por ahora, asumimos que son válidos si se proporcionan.

            var membresia = _mapper.Map<Membresia>(createDto);
            membresia.FechaFin = createDto.FechaInicio.AddMonths(tipoMembresia.DuracionMeses);
            membresia.FechaCompra = DateTime.UtcNow;

            // Validar el estado proporcionado
            if (!Enum.TryParse<EstadoMembresia>(createDto.Estado, true, out _))
            {
                throw new ArgumentException($"Estado de membresía '{createDto.Estado}' no es válido.");
            }
            membresia.Estado = createDto.Estado;


            _context.Membresias.Add(membresia);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<MembresiaDto>(membresia);
            resultDto.NombreTipoMembresia = tipoMembresia.Nombre; // Añadir el nombre para el DTO
            return resultDto;
        }

        public async Task<IEnumerable<MembresiaDto>> GetAllAsync()
        {
            var membresias = await _context.Membresias
                                        .Include(m => m.TipoMembresia) // Para obtener NombreTipoMembresia
                                        .ToListAsync();
            return _mapper.Map<IEnumerable<MembresiaDto>>(membresias);
        }

        public async Task<MembresiaDto?> GetByIdAsync(int id)
        {
            var membresia = await _context.Membresias
                                        .Include(m => m.TipoMembresia)
                                        .FirstOrDefaultAsync(m => m.IdMembresia == id);
            return membresia == null ? null : _mapper.Map<MembresiaDto>(membresia);
        }

        public async Task<IEnumerable<MembresiaDto>> GetByUsuarioIdAsync(int usuarioId)
        {
            var membresias = await _context.Membresias
                                        .Where(m => m.IdUsuario == usuarioId)
                                        .Include(m => m.TipoMembresia)
                                        .ToListAsync();
            return _mapper.Map<IEnumerable<MembresiaDto>>(membresias);
        }

        public async Task<bool> UpdateAsync(int id, UpdateMembresiaDto updateDto)
        {
            var membresia = await _context.Membresias.FindAsync(id);
            if (membresia == null) return false;

            if (updateDto.Estado != null)
            {
                if (!Enum.TryParse<EstadoMembresia>(updateDto.Estado, true, out _))
                {
                    throw new ArgumentException($"Estado de membresía '{updateDto.Estado}' no es válido.");
                }
                membresia.Estado = updateDto.Estado;
            }
            if (updateDto.AutoRenovar.HasValue) membresia.AutoRenovar = updateDto.AutoRenovar.Value;
            if (updateDto.FechaFin.HasValue) membresia.FechaFin = updateDto.FechaFin.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id)
        {
            var membresia = await _context.Membresias.FindAsync(id);
            if (membresia == null) return false;

            membresia.Estado = EstadoMembresia.Cancelada.ToString();
            membresia.AutoRenovar = false; // Asegurarse de que no se auto-renueve
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MembresiaDto?> RenewAsync(int id, DateOnly nuevaFechaInicio)
        {
            var membresia = await _context.Membresias
                                    .Include(m => m.TipoMembresia)
                                    .FirstOrDefaultAsync(m => m.IdMembresia == id);
            
            if (membresia == null || membresia.TipoMembresia == null) return null;

            // Lógica de renovación:
            // Usualmente se crea una *nueva* membresía o se actualiza la fecha de fin de la actual.
            // Aquí actualizaremos la existente, asumiendo que es una renovación directa.
            // Si el tipo de membresía cambió, sería más como crear una nueva.

            membresia.FechaInicio = nuevaFechaInicio;
            membresia.FechaFin = nuevaFechaInicio.AddMonths(membresia.TipoMembresia.DuracionMeses);
            membresia.Estado = EstadoMembresia.Activa.ToString(); // O PendientePago si requiere pago
            membresia.FechaCompra = DateTime.UtcNow; // Actualizar fecha de "compra" de la renovación

            await _context.SaveChangesAsync();
            return _mapper.Map<MembresiaDto>(membresia);
        }
    }
}