using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.Notificacion;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Common;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacionController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IReferenceValidationService _referenceValidationService;

        public NotificacionController(
            ApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IReferenceValidationService referenceValidationService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _referenceValidationService = referenceValidationService;
        }

        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<NotificacionDto>>> GetNotificaciones(
            [FromQuery] string? idUsuarioDestino = null,
            [FromQuery] bool soloNoLeidas = false,
            [FromQuery] int? idTicket = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 10)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            ValidarRangoFechas(fechaDesde, fechaHasta);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var query = CrearQueryNotificaciones();
            query = AplicarFiltroAcceso(query, idUsuarioDestino);
            query = AplicarFiltros(query, soloNoLeidas, idTicket, fechaDesde, fechaHasta);

            var totalRegistros = await query.CountAsync();

            var notificaciones = await query
                .OrderByDescending(n => n.FechaCreacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<NotificacionDto>>(notificaciones));

            return Ok(response);
        }

        [HttpGet("mis-notificaciones")]
        public async Task<ActionResult<ResultadoPaginadoDto<NotificacionDto>>> GetMisNotificaciones(
            [FromQuery] bool soloNoLeidas = false,
            [FromQuery] int? idTicket = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 10)
        {
            var idUsuario = _currentUserService.IdUsuario;

            if (idUsuario is null)
            {
                return Unauthorized();
            }

            return await GetNotificaciones(idUsuario, soloNoLeidas, idTicket, fechaDesde, fechaHasta, pagina, tamanoPagina);
        }

        [HttpGet("no-leidas/count")]
        public async Task<ActionResult<NotificacionConteoDto>> GetTotalNoLeidas([FromQuery] string? idUsuarioDestino = null)
        {
            var query = CrearQueryNotificaciones();
            query = AplicarFiltroAcceso(query, idUsuarioDestino);

            var total = await query.CountAsync(n => !n.Leida);

            return Ok(new NotificacionConteoDto { Total = total });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NotificacionDto>> GetNotificacion([FromRoute] int id)
        {
            var notificacion = await _context.Notificaciones
                .Include(n => n.UsuarioDestino)
                .Include(n => n.Ticket)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.IdNotificacion == id);

            if (notificacion is null)
            {
                return NotFound();
            }

            if (!PuedeVerNotificacion(notificacion))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<NotificacionDto>(notificacion));
        }

        [HttpPost]
        [Authorize(Roles = $"{AppRoles.Administrador},{AppRoles.ResolvedorTicket}")]
        public async Task<ActionResult<NotificacionDto>> CreateNotificacion([FromBody] CrearNotificacionDto request)
        {
            Normalizar(request);

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            var notificacion = _mapper.Map<Notificacion>(request);

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            await _context.Entry(notificacion).Reference(n => n.UsuarioDestino).LoadAsync();
            await _context.Entry(notificacion).Reference(n => n.Ticket).LoadAsync();

            var response = _mapper.Map<NotificacionDto>(notificacion);

            return CreatedAtAction(nameof(GetNotificacion), new { id = notificacion.IdNotificacion }, response);
        }

        [HttpPut("{id:int}/marcar-leida")]
        public async Task<IActionResult> MarcarLeida([FromRoute] int id)
        {
            var notificacion = await _context.Notificaciones.FindAsync(id);

            if (notificacion is null)
            {
                return NotFound();
            }

            if (!PuedeVerNotificacion(notificacion))
            {
                return Forbid();
            }

            if (!notificacion.Leida)
            {
                notificacion.Leida = true;
                notificacion.FechaLectura = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpPut("{id:int}/marcar-no-leida")]
        public async Task<IActionResult> MarcarNoLeida([FromRoute] int id)
        {
            var notificacion = await _context.Notificaciones.FindAsync(id);

            if (notificacion is null)
            {
                return NotFound();
            }

            if (!PuedeVerNotificacion(notificacion))
            {
                return Forbid();
            }

            if (notificacion.Leida)
            {
                notificacion.Leida = false;
                notificacion.FechaLectura = null;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpPut("marcar-leidas")]
        public async Task<IActionResult> MarcarLeidas([FromBody] MarcarNotificacionesDto request)
        {
            var query = _context.Notificaciones
                .Where(n => request.IdNotificaciones.Contains(n.IdNotificacion));

            query = AplicarFiltroAcceso(query, null);

            var notificaciones = await query.ToListAsync();
            var fechaLectura = DateTime.Now;

            foreach (var notificacion in notificaciones)
            {
                notificacion.Leida = true;
                notificacion.FechaLectura ??= fechaLectura;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("marcar-todas-leidas")]
        public async Task<IActionResult> MarcarTodasLeidas([FromQuery] string? idUsuarioDestino = null)
        {
            var usuario = ResolverUsuarioDestino(idUsuarioDestino);

            if (usuario is null)
            {
                return Unauthorized();
            }

            if (!await _referenceValidationService.UsuarioActivoExiste(usuario))
            {
                ModelState.AddModelError(nameof(idUsuarioDestino), "El usuario destino no existe o esta inactivo.");
                return ValidationProblem(ModelState);
            }

            var notificaciones = await _context.Notificaciones
                .Where(n => n.IdUsuarioDestino == usuario && !n.Leida)
                .ToListAsync();

            foreach (var notificacion in notificaciones)
            {
                notificacion.Leida = true;
                notificacion.FechaLectura = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNotificacion([FromRoute] int id)
        {
            var notificacion = await _context.Notificaciones.FindAsync(id);

            if (notificacion is null)
            {
                return NotFound();
            }

            if (!PuedeVerNotificacion(notificacion))
            {
                return Forbid();
            }

            _context.Notificaciones.Remove(notificacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ReferenciasValidas(CrearNotificacionDto request)
        {
            var esValido = true;

            if (!await _referenceValidationService.UsuarioActivoExiste(request.IdUsuarioDestino))
            {
                ModelState.AddModelError(nameof(request.IdUsuarioDestino), "El usuario destino no existe o esta inactivo.");
                esValido = false;
            }

            if (request.IdTicket.HasValue &&
                !await _referenceValidationService.TicketExiste(request.IdTicket.Value))
            {
                ModelState.AddModelError(nameof(request.IdTicket), "El ticket indicado no existe.");
                esValido = false;
            }

            return esValido;
        }

        private static void Normalizar(CrearNotificacionDto request)
        {
            request.IdUsuarioDestino = request.IdUsuarioDestino.Trim();
            request.Titulo = request.Titulo.Trim();
            request.Mensaje = request.Mensaje.Trim();
        }

        private IQueryable<Notificacion> CrearQueryNotificaciones()
        {
            return _context.Notificaciones
                .Include(n => n.UsuarioDestino)
                .Include(n => n.Ticket)
                .AsNoTracking();
        }

        private IQueryable<Notificacion> AplicarFiltroAcceso(IQueryable<Notificacion> query, string? idUsuarioDestino)
        {
            var idUsuario = _currentUserService.IdUsuario;

            if (_currentUserService.EsAdministrador)
            {
                if (!string.IsNullOrWhiteSpace(idUsuarioDestino))
                {
                    var usuario = idUsuarioDestino.Trim();
                    query = query.Where(n => n.IdUsuarioDestino == usuario);
                }

                return query;
            }

            if (idUsuario is not null)
            {
                query = query.Where(n => n.IdUsuarioDestino == idUsuario);
            }
            else
            {
                query = query.Where(n => false);
            }

            return query;
        }

        private static IQueryable<Notificacion> AplicarFiltros(
            IQueryable<Notificacion> query,
            bool soloNoLeidas,
            int? idTicket,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            if (soloNoLeidas)
            {
                query = query.Where(n => !n.Leida);
            }

            if (idTicket.HasValue)
            {
                query = query.Where(n => n.IdTicket == idTicket.Value);
            }

            if (fechaDesde.HasValue)
            {
                query = query.Where(n => n.FechaCreacion >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(n => n.FechaCreacion <= fechaHasta.Value);
            }

            return query;
        }

        private bool PuedeVerNotificacion(Notificacion notificacion)
        {
            var idUsuario = _currentUserService.IdUsuario;

            return _currentUserService.EsAdministrador ||
                (idUsuario is not null && notificacion.IdUsuarioDestino == idUsuario);
        }

        private string? ResolverUsuarioDestino(string? idUsuarioDestino)
        {
            if (_currentUserService.EsAdministrador && !string.IsNullOrWhiteSpace(idUsuarioDestino))
            {
                return idUsuarioDestino.Trim();
            }

            return _currentUserService.IdUsuario;
        }

        private void ValidarRangoFechas(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde.Value > fechaHasta.Value)
            {
                ModelState.AddModelError(nameof(fechaDesde), "La fecha desde no puede ser mayor que la fecha hasta.");
            }
        }
    }
}
