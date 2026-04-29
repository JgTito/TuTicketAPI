using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.TicketRelacion;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Tickets;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketRelacionController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITicketAccessService _ticketAccessService;

        public TicketRelacionController(ApplicationDbContext context, IMapper mapper, ITicketAccessService ticketAccessService)
        {
            _context = context;
            _mapper = mapper;
            _ticketAccessService = ticketAccessService;
        }

        [HttpGet("/api/Ticket/{idTicket:int}/relaciones")]
        public async Task<ActionResult<IEnumerable<TicketRelacionDto>>> GetRelacionesPorTicket(
            [FromRoute] int idTicket,
            [FromQuery] bool incluirInactivos = false)
        {
            if (!await _ticketAccessService.PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            var query = RelacionesConReferencias()
                .AsNoTracking()
                .Where(r => r.IdTicketOrigen == idTicket || r.IdTicketRelacionado == idTicket);

            if (!incluirInactivos)
            {
                query = query.Where(r => r.Activo);
            }

            var relaciones = await query
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TicketRelacionDto>>(relaciones));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketRelacionDto>> GetTicketRelacion([FromRoute] int id)
        {
            var relacion = await RelacionesConReferencias()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdTicketRelacion == id);

            if (relacion is null)
            {
                return NotFound();
            }

            if (!await PuedeVerRelacion(relacion))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TicketRelacionDto>(relacion));
        }

        [HttpPost("/api/Ticket/{idTicket:int}/relaciones")]
        public async Task<ActionResult<TicketRelacionDto>> CreateTicketRelacion([FromRoute] int idTicket, [FromBody] CrearTicketRelacionDto request)
        {
            Normalizar(request);

            if (!await _ticketAccessService.PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            if (!await ReferenciasValidas(idTicket, request))
            {
                return ValidationProblem(ModelState);
            }

            if (await ExisteRelacionActiva(idTicket, request.IdTicketRelacionado, request.IdTipoRelacionTicket))
            {
                ModelState.AddModelError(nameof(request.IdTicketRelacionado), "Ya existe una relacion activa entre los tickets con ese tipo.");
                return ValidationProblem(ModelState);
            }

            var relacion = _mapper.Map<TicketRelacion>(request);
            relacion.IdTicketOrigen = idTicket;

            _context.TicketRelaciones.Add(relacion);
            await _context.SaveChangesAsync();

            await CargarReferencias(relacion);

            var response = _mapper.Map<TicketRelacionDto>(relacion);

            return CreatedAtAction(nameof(GetTicketRelacion), new { id = relacion.IdTicketRelacion }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTicketRelacion([FromRoute] int id, [FromBody] ActualizarTicketRelacionDto request)
        {
            var relacion = await _context.TicketRelaciones.FindAsync(id);

            if (relacion is null)
            {
                return NotFound();
            }

            if (!await PuedeVerRelacion(relacion))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            if (request.Activo && await ExisteRelacionActiva(request.IdTicketOrigen, request.IdTicketRelacionado, request.IdTipoRelacionTicket, id))
            {
                ModelState.AddModelError(nameof(request.IdTicketRelacionado), "Ya existe una relacion activa entre los tickets con ese tipo.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, relacion);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTicketRelacion([FromRoute] int id)
        {
            var relacion = await _context.TicketRelaciones.FindAsync(id);

            if (relacion is null)
            {
                return NotFound();
            }

            if (!await PuedeVerRelacion(relacion))
            {
                return Forbid();
            }

            if (!relacion.Activo)
            {
                return NoContent();
            }

            relacion.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<TicketRelacion> RelacionesConReferencias()
        {
            return _context.TicketRelaciones
                .Include(r => r.TicketOrigen)
                .Include(r => r.TicketRelacionado)
                .Include(r => r.TipoRelacionTicket)
                .Include(r => r.UsuarioCreacion);
        }

        private async Task<bool> ReferenciasValidas(int idTicketOrigen, CrearTicketRelacionDto request)
        {
            var esValido = true;

            if (idTicketOrigen == request.IdTicketRelacionado)
            {
                ModelState.AddModelError(nameof(request.IdTicketRelacionado), "El ticket no puede relacionarse consigo mismo.");
                return false;
            }

            if (!await _context.Tickets.AnyAsync(t => t.IdTicket == idTicketOrigen && t.Activo))
            {
                ModelState.AddModelError(nameof(idTicketOrigen), "El ticket origen no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _context.Tickets.AnyAsync(t => t.IdTicket == request.IdTicketRelacionado && t.Activo))
            {
                ModelState.AddModelError(nameof(request.IdTicketRelacionado), "El ticket relacionado no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _context.TipoRelacionTickets.AnyAsync(t => t.IdTipoRelacionTicket == request.IdTipoRelacionTicket && t.Activo))
            {
                ModelState.AddModelError(nameof(request.IdTipoRelacionTicket), "El tipo de relacion indicado no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _context.Users.AnyAsync(u => u.Id == request.IdUsuarioCreacion && u.Activo))
            {
                ModelState.AddModelError(nameof(request.IdUsuarioCreacion), "El usuario indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private async Task<bool> ReferenciasValidas(ActualizarTicketRelacionDto request)
        {
            var esValido = true;

            if (request.IdTicketOrigen == request.IdTicketRelacionado)
            {
                ModelState.AddModelError(nameof(request.IdTicketRelacionado), "El ticket no puede relacionarse consigo mismo.");
                return false;
            }

            if (!await _context.Tickets.AnyAsync(t => t.IdTicket == request.IdTicketOrigen && t.Activo))
            {
                ModelState.AddModelError(nameof(request.IdTicketOrigen), "El ticket origen no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _context.Tickets.AnyAsync(t => t.IdTicket == request.IdTicketRelacionado && t.Activo))
            {
                ModelState.AddModelError(nameof(request.IdTicketRelacionado), "El ticket relacionado no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _context.TipoRelacionTickets.AnyAsync(t => t.IdTipoRelacionTicket == request.IdTipoRelacionTicket && t.Activo))
            {
                ModelState.AddModelError(nameof(request.IdTipoRelacionTicket), "El tipo de relacion indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private Task<bool> ExisteRelacionActiva(int idTicketOrigen, int idTicketRelacionado, int idTipoRelacionTicket, int? idTicketRelacionExcluir = null)
        {
            return _context.TicketRelaciones.AnyAsync(r =>
                r.Activo &&
                r.IdTicketOrigen == idTicketOrigen &&
                r.IdTicketRelacionado == idTicketRelacionado &&
                r.IdTipoRelacionTicket == idTipoRelacionTicket &&
                (!idTicketRelacionExcluir.HasValue || r.IdTicketRelacion != idTicketRelacionExcluir.Value));
        }

        private async Task CargarReferencias(TicketRelacion relacion)
        {
            await _context.Entry(relacion).Reference(r => r.TicketOrigen).LoadAsync();
            await _context.Entry(relacion).Reference(r => r.TicketRelacionado).LoadAsync();
            await _context.Entry(relacion).Reference(r => r.TipoRelacionTicket).LoadAsync();
            await _context.Entry(relacion).Reference(r => r.UsuarioCreacion).LoadAsync();
        }

        private static void Normalizar(CrearTicketRelacionDto request)
        {
            request.IdUsuarioCreacion = request.IdUsuarioCreacion.Trim();
            request.Observacion = string.IsNullOrWhiteSpace(request.Observacion) ? null : request.Observacion.Trim();
        }

        private static void Normalizar(ActualizarTicketRelacionDto request)
        {
            request.Observacion = string.IsNullOrWhiteSpace(request.Observacion) ? null : request.Observacion.Trim();
        }

        private async Task<bool> PuedeVerRelacion(TicketRelacion relacion)
        {
            if (User.IsInRole(AppRoles.Administrador))
            {
                return true;
            }

            return await _ticketAccessService.PuedeVerTicket(relacion.IdTicketOrigen) ||
                await _ticketAccessService.PuedeVerTicket(relacion.IdTicketRelacionado);
        }
    }
}
