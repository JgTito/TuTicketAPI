using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.TicketBitacora;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Tickets;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketBitacoraController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITicketAccessService _ticketAccessService;

        public TicketBitacoraController(ApplicationDbContext context, IMapper mapper, ITicketAccessService ticketAccessService)
        {
            _context = context;
            _mapper = mapper;
            _ticketAccessService = ticketAccessService;
        }

        [HttpGet("/api/Ticket/{idTicket:int}/bitacora")]
        public async Task<ActionResult<ResultadoPaginadoDto<TicketBitacoraDto>>> GetBitacorasPorTicket(
            [FromRoute] int idTicket,
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] bool? esInterno = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            if (!await _ticketAccessService.PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            var query = _context.TicketBitacoras
                .Include(b => b.UsuarioCreacion)
                .AsNoTracking()
                .Where(b => b.IdTicket == idTicket);

            if (!incluirInactivos)
            {
                query = query.Where(b => b.Activo);
            }

            if (esInterno.HasValue)
            {
                query = query.Where(b => b.EsInterno == esInterno.Value);
            }

            var totalRegistros = await query.CountAsync();

            var bitacoras = await query
                .OrderByDescending(b => b.FechaCreacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<TicketBitacoraDto>>(bitacoras));

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketBitacoraDto>> GetTicketBitacora([FromRoute] int id)
        {
            var bitacora = await _context.TicketBitacoras
                .Include(b => b.UsuarioCreacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.IdTicketBitacora == id);

            if (bitacora is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(bitacora.IdTicket))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TicketBitacoraDto>(bitacora));
        }

        [HttpPost("/api/Ticket/{idTicket:int}/bitacora")]
        public async Task<ActionResult<TicketBitacoraDto>> CreateTicketBitacora([FromRoute] int idTicket, [FromBody] CrearTicketBitacoraDto request)
        {
            Normalizar(request);

            if (!await _ticketAccessService.PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            if (!await _context.Tickets.AnyAsync(t => t.IdTicket == idTicket && t.Activo))
            {
                ModelState.AddModelError(nameof(idTicket), "El ticket indicado no existe o esta inactivo.");
                return ValidationProblem(ModelState);
            }

            if (!await _context.Users.AnyAsync(u => u.Id == request.IdUsuarioCreacion && u.Activo))
            {
                ModelState.AddModelError(nameof(request.IdUsuarioCreacion), "El usuario indicado no existe o esta inactivo.");
                return ValidationProblem(ModelState);
            }

            var bitacora = _mapper.Map<TicketBitacora>(request);
            bitacora.IdTicket = idTicket;

            _context.TicketBitacoras.Add(bitacora);
            await _context.SaveChangesAsync();

            await _context.Entry(bitacora).Reference(b => b.UsuarioCreacion).LoadAsync();

            var response = _mapper.Map<TicketBitacoraDto>(bitacora);

            return CreatedAtAction(nameof(GetTicketBitacora), new { id = bitacora.IdTicketBitacora }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTicketBitacora([FromRoute] int id, [FromBody] ActualizarTicketBitacoraDto request)
        {
            var bitacora = await _context.TicketBitacoras.FindAsync(id);

            if (bitacora is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(bitacora.IdTicket))
            {
                return Forbid();
            }

            Normalizar(request);
            _mapper.Map(request, bitacora);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTicketBitacora([FromRoute] int id)
        {
            var bitacora = await _context.TicketBitacoras.FindAsync(id);

            if (bitacora is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(bitacora.IdTicket))
            {
                return Forbid();
            }

            if (!bitacora.Activo)
            {
                return NoContent();
            }

            bitacora.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearTicketBitacoraDto request)
        {
            request.Comentario = request.Comentario.Trim();
            request.IdUsuarioCreacion = request.IdUsuarioCreacion.Trim();
        }

        private static void Normalizar(ActualizarTicketBitacoraDto request)
        {
            request.Comentario = request.Comentario.Trim();
        }

    }
}
