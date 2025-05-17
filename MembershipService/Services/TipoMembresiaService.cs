using AutoMapper;
using MembershipService.Data;
using MembershipService.DTOs;
using MembershipService.Interfaces;
using MembershipService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MembershipService.Services
{
    public class TipoMembresiaService : ITipoMembresiaService
    {
        private readonly MembershipDbContext _context;
        private readonly IMapper _mapper;

        public TipoMembresiaService(MembershipDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TipoMembresiaDto?> CreateAsync(CreateTipoMembresiaDto createDto)
        {
            var tipoMembresia = _mapper.Map<TipoMembresia>(createDto);
            _context.TiposMembresia.Add(tipoMembresia);
            await _context.SaveChangesAsync();
            return _mapper.Map<TipoMembresiaDto>(tipoMembresia);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tipoMembresia = await _context.TiposMembresia.FindAsync(id);
            if (tipoMembresia == null) return false;

            tipoMembresia.Activo = false;            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TipoMembresiaDto>> GetAllAsync()
        {
            var tiposMembresia = await _context.TiposMembresia.Where(t => t.Activo).ToListAsync();
            return _mapper.Map<IEnumerable<TipoMembresiaDto>>(tiposMembresia);
        }

        public async Task<TipoMembresiaDto?> GetByIdAsync(int id)
        {
            var tipoMembresia = await _context.TiposMembresia.FindAsync(id);
            return tipoMembresia == null ? null : _mapper.Map<TipoMembresiaDto>(tipoMembresia);
        }

        public async Task<bool> UpdateAsync(int id, UpdateTipoMembresiaDto updateDto)
        {
            var tipoMembresia = await _context.TiposMembresia.FindAsync(id);
            if (tipoMembresia == null) return false;

            // Mapeo selectivo para no sobrescribir con nulls si no se proporcionan
            if (updateDto.Nombre != null) tipoMembresia.Nombre = updateDto.Nombre;
            if (updateDto.Descripcion != null) tipoMembresia.Descripcion = updateDto.Descripcion;
            if (updateDto.DuracionMeses.HasValue) tipoMembresia.DuracionMeses = updateDto.DuracionMeses.Value;
            if (updateDto.Precio.HasValue) tipoMembresia.Precio = updateDto.Precio.Value;
            if (updateDto.Activo.HasValue) tipoMembresia.Activo = updateDto.Activo.Value;
            
            //_mapper.Map(updateDto, tipoMembresia); // AutoMapper puede hacer esto si se configura bien

            await _context.SaveChangesAsync();
            return true;
        }
    }
}