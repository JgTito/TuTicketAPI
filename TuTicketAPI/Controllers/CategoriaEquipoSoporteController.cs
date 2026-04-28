using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.CategoriaEquipoSoporte;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Administrador)]
    public class CategoriaEquipoSoporteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriaEquipoSoporteController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaEquipoSoporteDto>>> GetCategoriaEquipoSoportes(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idCategoriaTicket = null,
            [FromQuery] int? idEquipoSoporte = null)
        {
            var query = _context.CategoriaEquipoSoportes
                .Include(c => c.CategoriaTicket)
                .Include(c => c.EquipoSoporte)
                .AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(c => c.Activo);
            }

            if (idCategoriaTicket.HasValue)
            {
                query = query.Where(c => c.IdCategoriaTicket == idCategoriaTicket.Value);
            }

            if (idEquipoSoporte.HasValue)
            {
                query = query.Where(c => c.IdEquipoSoporte == idEquipoSoporte.Value);
            }

            var categoriasEquipos = await query
                .OrderBy(c => c.CategoriaTicket.Nombre)
                .ThenBy(c => c.EquipoSoporte.Nombre)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CategoriaEquipoSoporteDto>>(categoriasEquipos));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaEquipoSoporteDto>> GetCategoriaEquipoSoporte([FromRoute] int id)
        {
            var categoriaEquipo = await _context.CategoriaEquipoSoportes
                .Include(c => c.CategoriaTicket)
                .Include(c => c.EquipoSoporte)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoriaEquipoSoporte == id);

            if (categoriaEquipo is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CategoriaEquipoSoporteDto>(categoriaEquipo));
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaEquipoSoporteDto>> CreateCategoriaEquipoSoporte([FromBody] CrearCategoriaEquipoSoporteDto request)
        {
            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            if (request.Activo && await ExisteEquipoActivoEnCategoria(request.IdCategoriaTicket, request.IdEquipoSoporte))
            {
                ModelState.AddModelError(nameof(request.IdEquipoSoporte), "El equipo de soporte indicado ya esta activo para la categoria.");
                return ValidationProblem(ModelState);
            }

            var categoriaEquipo = _mapper.Map<CategoriaEquipoSoporte>(request);

            _context.CategoriaEquipoSoportes.Add(categoriaEquipo);
            await _context.SaveChangesAsync();

            await CargarReferencias(categoriaEquipo);

            var response = _mapper.Map<CategoriaEquipoSoporteDto>(categoriaEquipo);

            return CreatedAtAction(nameof(GetCategoriaEquipoSoporte), new { id = categoriaEquipo.IdCategoriaEquipoSoporte }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategoriaEquipoSoporte([FromRoute] int id, [FromBody] ActualizarCategoriaEquipoSoporteDto request)
        {
            var categoriaEquipo = await _context.CategoriaEquipoSoportes.FindAsync(id);

            if (categoriaEquipo is null)
            {
                return NotFound();
            }

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            if (request.Activo && await ExisteEquipoActivoEnCategoria(request.IdCategoriaTicket, request.IdEquipoSoporte, id))
            {
                ModelState.AddModelError(nameof(request.IdEquipoSoporte), "El equipo de soporte indicado ya esta activo para la categoria.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, categoriaEquipo);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategoriaEquipoSoporte([FromRoute] int id)
        {
            var categoriaEquipo = await _context.CategoriaEquipoSoportes.FindAsync(id);

            if (categoriaEquipo is null)
            {
                return NotFound();
            }

            if (!categoriaEquipo.Activo)
            {
                return NoContent();
            }

            categoriaEquipo.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ReferenciasValidas(CrearCategoriaEquipoSoporteDto request)
        {
            var esValido = true;

            if (!await _context.CategoriaTickets.AnyAsync(c => c.IdCategoriaTicket == request.IdCategoriaTicket && c.Activo))
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada no existe o esta inactiva.");
                esValido = false;
            }

            if (!await _context.EquipoSoportes.AnyAsync(e => e.IdEquipoSoporte == request.IdEquipoSoporte && e.Activo))
            {
                ModelState.AddModelError(nameof(request.IdEquipoSoporte), "El equipo de soporte indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private async Task<bool> ReferenciasValidas(ActualizarCategoriaEquipoSoporteDto request)
        {
            var esValido = true;

            if (!await _context.CategoriaTickets.AnyAsync(c => c.IdCategoriaTicket == request.IdCategoriaTicket && c.Activo))
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada no existe o esta inactiva.");
                esValido = false;
            }

            if (!await _context.EquipoSoportes.AnyAsync(e => e.IdEquipoSoporte == request.IdEquipoSoporte && e.Activo))
            {
                ModelState.AddModelError(nameof(request.IdEquipoSoporte), "El equipo de soporte indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private Task<bool> ExisteEquipoActivoEnCategoria(int idCategoriaTicket, int idEquipoSoporte, int? idCategoriaEquipoSoporteExcluir = null)
        {
            return _context.CategoriaEquipoSoportes.AnyAsync(c =>
                c.Activo &&
                c.IdCategoriaTicket == idCategoriaTicket &&
                c.IdEquipoSoporte == idEquipoSoporte &&
                (!idCategoriaEquipoSoporteExcluir.HasValue || c.IdCategoriaEquipoSoporte != idCategoriaEquipoSoporteExcluir.Value));
        }

        private async Task CargarReferencias(CategoriaEquipoSoporte categoriaEquipo)
        {
            await _context.Entry(categoriaEquipo).Reference(c => c.CategoriaTicket).LoadAsync();
            await _context.Entry(categoriaEquipo).Reference(c => c.EquipoSoporte).LoadAsync();
        }
    }
}
