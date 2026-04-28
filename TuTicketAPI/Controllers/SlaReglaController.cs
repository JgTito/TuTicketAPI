using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.SlaRegla;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlaReglaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SlaReglaController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SlaReglaDto>>> GetSlaReglas(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idSlaPolitica = null,
            [FromQuery] int? idPrioridadTicket = null,
            [FromQuery] int? idCategoriaTicket = null)
        {
            var query = _context.SlaReglas
                .Include(s => s.SlaPolitica)
                .Include(s => s.PrioridadTicket)
                .Include(s => s.CategoriaTicket)
                .AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(s => s.Activo);
            }

            if (idSlaPolitica.HasValue)
            {
                query = query.Where(s => s.IdSlaPolitica == idSlaPolitica.Value);
            }

            if (idPrioridadTicket.HasValue)
            {
                query = query.Where(s => s.IdPrioridadTicket == idPrioridadTicket.Value);
            }

            if (idCategoriaTicket.HasValue)
            {
                query = query.Where(s => s.IdCategoriaTicket == idCategoriaTicket.Value);
            }

            var reglas = await query
                .OrderBy(s => s.SlaPolitica.Nombre)
                .ThenBy(s => s.PrioridadTicket.Nivel)
                .ThenBy(s => s.CategoriaTicket == null ? string.Empty : s.CategoriaTicket.Nombre)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<SlaReglaDto>>(reglas));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SlaReglaDto>> GetSlaRegla([FromRoute] int id)
        {
            var regla = await _context.SlaReglas
                .Include(s => s.SlaPolitica)
                .Include(s => s.PrioridadTicket)
                .Include(s => s.CategoriaTicket)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdSlaRegla == id);

            if (regla is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<SlaReglaDto>(regla));
        }

        [HttpPost]
        public async Task<ActionResult<SlaReglaDto>> CreateSlaRegla([FromBody] CrearSlaReglaDto request)
        {
            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            var regla = _mapper.Map<SlaRegla>(request);

            _context.SlaReglas.Add(regla);
            await _context.SaveChangesAsync();

            await CargarReferencias(regla);

            var response = _mapper.Map<SlaReglaDto>(regla);

            return CreatedAtAction(nameof(GetSlaRegla), new { id = regla.IdSlaRegla }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSlaRegla([FromRoute] int id, [FromBody] ActualizarSlaReglaDto request)
        {
            var regla = await _context.SlaReglas.FindAsync(id);

            if (regla is null)
            {
                return NotFound();
            }

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, regla);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSlaRegla([FromRoute] int id)
        {
            var regla = await _context.SlaReglas.FindAsync(id);

            if (regla is null)
            {
                return NotFound();
            }

            if (!regla.Activo)
            {
                return NoContent();
            }

            regla.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ReferenciasValidas(CrearSlaReglaDto request)
        {
            var esValido = true;

            if (!await _context.SlaPoliticas.AnyAsync(s => s.IdSlaPolitica == request.IdSlaPolitica))
            {
                ModelState.AddModelError(nameof(request.IdSlaPolitica), "La politica SLA indicada no existe.");
                esValido = false;
            }

            if (!await _context.PrioridadTickets.AnyAsync(p => p.IdPrioridadTicket == request.IdPrioridadTicket))
            {
                ModelState.AddModelError(nameof(request.IdPrioridadTicket), "La prioridad indicada no existe.");
                esValido = false;
            }

            if (request.IdCategoriaTicket.HasValue &&
                !await _context.CategoriaTickets.AnyAsync(c => c.IdCategoriaTicket == request.IdCategoriaTicket.Value))
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada no existe.");
                esValido = false;
            }

            return esValido;
        }

        private async Task<bool> ReferenciasValidas(ActualizarSlaReglaDto request)
        {
            var esValido = true;

            if (!await _context.SlaPoliticas.AnyAsync(s => s.IdSlaPolitica == request.IdSlaPolitica))
            {
                ModelState.AddModelError(nameof(request.IdSlaPolitica), "La politica SLA indicada no existe.");
                esValido = false;
            }

            if (!await _context.PrioridadTickets.AnyAsync(p => p.IdPrioridadTicket == request.IdPrioridadTicket))
            {
                ModelState.AddModelError(nameof(request.IdPrioridadTicket), "La prioridad indicada no existe.");
                esValido = false;
            }

            if (request.IdCategoriaTicket.HasValue &&
                !await _context.CategoriaTickets.AnyAsync(c => c.IdCategoriaTicket == request.IdCategoriaTicket.Value))
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada no existe.");
                esValido = false;
            }

            return esValido;
        }

        private async Task CargarReferencias(SlaRegla regla)
        {
            await _context.Entry(regla).Reference(s => s.SlaPolitica).LoadAsync();
            await _context.Entry(regla).Reference(s => s.PrioridadTicket).LoadAsync();
            await _context.Entry(regla).Reference(s => s.CategoriaTicket).LoadAsync();
        }
    }
}
