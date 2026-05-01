using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.TicketSla;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Common;
using TuTicketAPI.Services.Tickets;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketSlaController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITicketAccessService _ticketAccessService;
        private readonly IReferenceValidationService _referenceValidationService;

        public TicketSlaController(ApplicationDbContext context, IMapper mapper, ITicketAccessService ticketAccessService, IReferenceValidationService referenceValidationService)
        {
            _context = context;
            _mapper = mapper;
            _ticketAccessService = ticketAccessService;
            _referenceValidationService = referenceValidationService;
        }

        [HttpGet("/api/Ticket/{idTicket:int}/sla")]
        public async Task<ActionResult<IEnumerable<TicketSlaDto>>> GetSlasPorTicket(
            [FromRoute] int idTicket,
            [FromQuery] bool incluirInactivos = false)
        {
            if (!await _ticketAccessService.PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            var query = SlasConReferencias()
                .AsNoTracking()
                .Where(s => s.IdTicket == idTicket);

            if (!incluirInactivos)
            {
                query = query.Where(s => s.Activo);
            }

            var slas = await query
                .OrderByDescending(s => s.FechaInicio)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TicketSlaDto>>(slas));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketSlaDto>> GetTicketSla([FromRoute] int id)
        {
            var sla = await SlasConReferencias()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdTicketSla == id);

            if (sla is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(sla.IdTicket))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TicketSlaDto>(sla));
        }

        [HttpGet("vencidos")]
        public async Task<ActionResult<IEnumerable<TicketSlaDto>>> GetSlasVencidos([FromQuery] bool soloActivos = true)
        {
            var ahora = DateTime.Now;
            var query = SlasConReferencias().AsNoTracking();
            query = _ticketAccessService.AplicarFiltroAcceso(query);

            if (soloActivos)
            {
                query = query.Where(s => s.Activo);
            }

            var slas = await query
                .Where(s =>
                    (s.FechaPrimeraRespuestaReal == null && s.FechaLimitePrimeraRespuesta < ahora) ||
                    (s.FechaResolucionReal == null && s.FechaLimiteResolucion < ahora) ||
                    s.PrimeraRespuestaVencida ||
                    s.ResolucionVencida)
                .OrderBy(s => s.FechaLimiteResolucion)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TicketSlaDto>>(slas));
        }

        [HttpPut("{id:int}/registrar-primera-respuesta")]
        public async Task<IActionResult> RegistrarPrimeraRespuesta([FromRoute] int id, [FromBody] RegistrarTicketSlaFechaDto request)
        {
            var sla = await _context.TicketSlas
                .Include(s => s.Ticket)
                .FirstOrDefaultAsync(s => s.IdTicketSla == id);

            if (sla is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(sla.IdTicket))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await UsuarioActivoExiste(request.IdUsuarioModificacion, nameof(request.IdUsuarioModificacion)))
            {
                return ValidationProblem(ModelState);
            }

            var fecha = request.Fecha ?? DateTime.Now;

            sla.FechaPrimeraRespuestaReal = fecha;
            sla.PrimeraRespuestaVencida = fecha > sla.FechaLimitePrimeraRespuesta;
            sla.Ticket.FechaPrimeraRespuesta ??= fecha;
            sla.Ticket.FechaActualizacion = DateTime.Now;

            _context.TicketHistoriales.Add(CrearHistorial(sla.IdTicket, "FechaPrimeraRespuesta", null, fecha.ToString("O"), request.IdUsuarioModificacion, request.Comentario));

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}/registrar-resolucion")]
        public async Task<IActionResult> RegistrarResolucion([FromRoute] int id, [FromBody] RegistrarTicketSlaFechaDto request)
        {
            var sla = await _context.TicketSlas
                .Include(s => s.Ticket)
                .FirstOrDefaultAsync(s => s.IdTicketSla == id);

            if (sla is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(sla.IdTicket))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await UsuarioActivoExiste(request.IdUsuarioModificacion, nameof(request.IdUsuarioModificacion)))
            {
                return ValidationProblem(ModelState);
            }

            var fecha = request.Fecha ?? DateTime.Now;

            sla.FechaResolucionReal = fecha;
            sla.ResolucionVencida = fecha > sla.FechaLimiteResolucion;
            sla.Ticket.FechaResolucion ??= fecha;
            sla.Ticket.FechaActualizacion = DateTime.Now;

            _context.TicketHistoriales.Add(CrearHistorial(sla.IdTicket, "FechaResolucion", null, fecha.ToString("O"), request.IdUsuarioModificacion, request.Comentario));

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("actualizar-vencimientos")]
        public async Task<IActionResult> ActualizarVencimientos()
        {
            var ahora = DateTime.Now;

            var slas = await _context.TicketSlas
                .Where(s => s.Activo)
                .ToListAsync();

            foreach (var sla in slas)
            {
                sla.PrimeraRespuestaVencida = sla.FechaPrimeraRespuestaReal == null && sla.FechaLimitePrimeraRespuesta < ahora;
                sla.ResolucionVencida = sla.FechaResolucionReal == null && sla.FechaLimiteResolucion < ahora;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<TicketSla> SlasConReferencias()
        {
            return _context.TicketSlas
                .Include(s => s.Ticket)
                .Include(s => s.SlaRegla)
                    .ThenInclude(r => r.SlaPolitica)
                .Include(s => s.SlaRegla)
                    .ThenInclude(r => r.PrioridadTicket)
                .Include(s => s.SlaRegla)
                    .ThenInclude(r => r.CategoriaTicket);
        }

        private async Task<bool> UsuarioActivoExiste(string idUsuario, string campo)
        {
            if (!await _referenceValidationService.UsuarioActivoExiste(idUsuario))
            {
                ModelState.AddModelError(campo, "El usuario indicado no existe o esta inactivo.");
                return false;
            }

            return true;
        }

        private static TicketHistorial CrearHistorial(int idTicket, string campo, string? valorAnterior, string? valorNuevo, string idUsuarioModificacion, string? comentario)
        {
            return new TicketHistorial
            {
                IdTicket = idTicket,
                CampoModificado = campo,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                IdUsuarioModificacion = idUsuarioModificacion,
                Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
            };
        }

        private static void Normalizar(RegistrarTicketSlaFechaDto request)
        {
            request.IdUsuarioModificacion = request.IdUsuarioModificacion.Trim();
            request.Comentario = string.IsNullOrWhiteSpace(request.Comentario) ? null : request.Comentario.Trim();
        }

    }
}
