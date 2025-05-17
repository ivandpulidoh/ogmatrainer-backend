using MembershipService.DTOs;
using MembershipService.Interfaces;
using MembershipService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MembershipService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiposMembresiaController : ControllerBase
    {
        private readonly ITipoMembresiaService _service;

        public TiposMembresiaController(ITipoMembresiaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoMembresiaDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoMembresiaDto>> GetById(int id)
        {
            var tipo = await _service.GetByIdAsync(id);
            if (tipo == null) return NotFound();
            return Ok(tipo);
        }

        [HttpPost]
        public async Task<ActionResult<TipoMembresiaDto>> Create(CreateTipoMembresiaDto createDto)
        {
            var nuevoTipo = await _service.CreateAsync(createDto);
            if (nuevoTipo == null) return BadRequest("No se pudo crear el tipo de membresía."); // o un error más específico
            return CreatedAtAction(nameof(GetById), new { id = nuevoTipo.IdTipoMembresia }, nuevoTipo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTipoMembresiaDto updateDto)
        {
            var success = await _service.UpdateAsync(id, updateDto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}