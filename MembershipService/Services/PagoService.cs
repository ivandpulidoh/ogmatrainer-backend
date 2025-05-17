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
    public class PagoService : IPagoService
    {
        private readonly MembershipDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PagoService> _logger;

        public PagoService(MembershipDbContext context, IMapper mapper, ILogger<PagoService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagoDto?> CreatePagoAsync(CreatePagoDto createPagoDto, int idUsuarioSolicitante)
        {                       
            
            var membresia = await _context.Membresias
                                    .Include(m => m.TipoMembresia)
                                    .FirstOrDefaultAsync(m => m.IdMembresia == createPagoDto.IdMembresia && m.IdUsuario == createPagoDto.IdUsuario);

            if (membresia == null)
            {
                _logger.LogWarning("Intento de pago para membresía inexistente o de otro usuario. MembresiaId: {MembresiaId}, UsuarioId: {UsuarioId}",
                    createPagoDto.IdMembresia, createPagoDto.IdUsuario);
                throw new ArgumentException("Membresía no encontrada o no pertenece al usuario especificado.");
            }
                       

            var pago = _mapper.Map<Pago>(createPagoDto);
            pago.FechaPago = DateTime.UtcNow;
            pago.EstadoPago = EstadoPago.Completado.ToString();
            pago.IdTransaccionExterna = $"SIM_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12)}";
                                    
            if (createPagoDto.Monto <= 0 && membresia.TipoMembresia != null)
            {
                pago.Monto = membresia.TipoMembresia.Precio;
            }
            else if (createPagoDto.Monto <= 0)
            {
                throw new ArgumentException("El monto del pago debe ser mayor a cero o el tipo de membresía debe tener un precio definido.");
            }


            _context.Pagos.Add(pago);
            
            if (pago.EstadoPago == EstadoPago.Completado.ToString())
            {
                if (membresia.Estado == EstadoMembresia.PendientePago.ToString())
                {
                    membresia.Estado = EstadoMembresia.Activa.ToString();
                    
                    if (membresia.FechaInicio <= DateOnly.FromDateTime(DateTime.UtcNow) && membresia.TipoMembresia != null)
                    {                         
                         membresia.FechaFin = membresia.FechaInicio.AddMonths(membresia.TipoMembresia.DuracionMeses);
                    }
                }                
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<PagoDto>(pago);
        }

        public async Task<IEnumerable<PagoDto>> GetAllPagosAsync(int pageNumber, int pageSize)
        {            
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var pagos = await _context.Pagos
                                .OrderByDescending(p => p.FechaPago)
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
            return _mapper.Map<IEnumerable<PagoDto>>(pagos);
        }

        public async Task<PagoDto?> GetPagoByIdAsync(int idPago)
        {
            var pago = await _context.Pagos.FindAsync(idPago);
            return pago == null ? null : _mapper.Map<PagoDto>(pago);
        }

        public async Task<IEnumerable<PagoDto>> GetPagosByUsuarioAsync(int idUsuario, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var pagos = await _context.Pagos
                                .Where(p => p.IdUsuario == idUsuario)
                                .OrderByDescending(p => p.FechaPago)
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
            return _mapper.Map<IEnumerable<PagoDto>>(pagos);
        }

        public async Task<bool> UpdatePagoStatusAsync(int idPago, UpdatePagoStatusDto updateStatusDto)
        {
            var pago = await _context.Pagos.FindAsync(idPago);
            if (pago == null) return false;

            if (!Enum.TryParse<EstadoPago>(updateStatusDto.EstadoPago, true, out var nuevoEstado))
            {
                throw new ArgumentException($"Estado de pago '{updateStatusDto.EstadoPago}' no es válido.");
            }

            pago.EstadoPago = nuevoEstado.ToString();
            if (!string.IsNullOrEmpty(updateStatusDto.IdTransaccionExterna))
            {
                pago.IdTransaccionExterna = updateStatusDto.IdTransaccionExterna;
            }

            
            if (pago.IdMembresia.HasValue && (nuevoEstado == EstadoPago.Fallido || nuevoEstado == EstadoPago.Reembolsado))
            {
                var membresia = await _context.Membresias.FindAsync(pago.IdMembresia.Value);
                if (membresia != null && membresia.Estado == EstadoMembresia.Activa.ToString())
                {
                    membresia.Estado = EstadoMembresia.PendientePago.ToString(); // O PendientePago si se espera reintento
                    _logger.LogInformation("Membresía {MembresiaId} actualizada a {Estado} debido a cambio de estado del pago {PagoId}",
                        membresia.IdMembresia, membresia.Estado, pago.IdPago);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}