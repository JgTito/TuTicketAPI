using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.TicketSla;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketSlaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TicketSlaController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("/api/Ticket/{idTicket:int}/sla")]
        public async Task<ActionResult<IEnumerable<TicketSlaDto>>> GetSlasPorTicket(
            [FromRoute] int idTicket,
            [FromQuery] bool incluirInactivos = false)
        {
            if (!await PuedeVerTicket(idTicket))
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

            if (!await PuedeVerTicket(sla.IdTicket))
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
            query = AplicarFiltroAcceso(query);

            if (soloActivos)
            {
                query = query.Where(s => s.Activo && s.Ticket.Activo);
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

            if (!await PuedeVerTicket(sla.IdTicket))
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

            if (!await PuedeVerTicket(sla.IdTicket))
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

        private IQueryable<TicketSla> AplicarFiltroAcceso(IQueryable<TicketSla> query)
        {
            var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole(AppRoles.Administrador))
            {
                return query;
            }

            if (idUsuario is null)
            {
                return query.Where(s => false);
            }

            if (EsSolicitanteSinPrivilegios())
            {
                query = query.Where(s => s.Ticket.IdUsuarioSolicitante == idUsuario);
            }
            else if (EsResolvedorSinAdministrador())
            {
                query = query.Where(s =>
                    s.Ticket.IdUsuarioAsignado == idUsuario ||
                    _context.EquipoSoporteUsuarios.Any(eu =>
                        eu.Activo &&
                        eu.IdUsuario == idUsuario &&
                        _context.CategoriaEquipoSoportes.Any(ce =>
                            ce.Activo &&
                            ce.IdEquipoSoporte == eu.IdEquipoSoporte &&
                            ce.IdCategoriaTicket == s.Ticket.SubcategoriaTicket.IdCategoriaTicket)));
            }
            else
            {
                query = query.Where(s => false);
            }

            return query;
        }

        private async Task<bool> UsuarioActivoExiste(string idUsuario, string campo)
        {
            if (string.IsNullOrWhiteSpace(idUsuario) ||
                !await _context.Users.AnyAsync(u => u.Id == idUsuario && u.Activo))
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

        private async Task<bool> PuedeVerTicket(int idTicket)
        {
            if (User.IsInRole(AppRoles.Administrador))
            {
                return await _context.Tickets.AnyAsync(t => t.IdTicket == idTicket);
            }

            var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (idUsuario is null)
            {
                return false;
            }

            if (EsSolicitanteSinPrivilegios())
            {
                return await _context.Tickets.AnyAsync(t => t.IdTicket == idTicket && t.IdUsuarioSolicitante == idUsuario);
            }

            if (EsResolvedorSinAdministrador())
            {
                return await _context.Tickets.AnyAsync(t =>
                    t.IdTicket == idTicket &&
                    (t.IdUsuarioAsignado == idUsuario ||
                        _context.EquipoSoporteUsuarios.Any(eu =>
                            eu.Activo &&
                            eu.IdUsuario == idUsuario &&
                            _context.CategoriaEquipoSoportes.Any(ce =>
                                ce.Activo &&
                                ce.IdEquipoSoporte == eu.IdEquipoSoporte &&
                                ce.IdCategoriaTicket == t.SubcategoriaTicket.IdCategoriaTicket))));
            }

            return false;
        }

        private bool EsSolicitanteSinPrivilegios()
        {
            return User.IsInRole(AppRoles.Solicitante) &&
                !User.IsInRole(AppRoles.Administrador) &&
                !User.IsInRole(AppRoles.ResolvedorTicket);
        }

        private bool EsResolvedorSinAdministrador()
        {
            return User.IsInRole(AppRoles.ResolvedorTicket) &&
                !User.IsInRole(AppRoles.Administrador);
        }
    }
}
