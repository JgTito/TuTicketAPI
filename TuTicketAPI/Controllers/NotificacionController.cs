using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Notificacion;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacionController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public NotificacionController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificacionDto>>> GetNotificaciones(
            [FromQuery] string? idUsuarioDestino = null,
            [FromQuery] bool soloNoLeidas = false,
            [FromQuery] int? idTicket = null)
        {
            var query = _context.Notificaciones
                .Include(n => n.UsuarioDestino)
                .Include(n => n.Ticket)
                .AsNoTracking();
            query = AplicarFiltroSolicitante(query);

            if (!string.IsNullOrWhiteSpace(idUsuarioDestino))
            {
                var usuario = idUsuarioDestino.Trim();
                query = query.Where(n => n.IdUsuarioDestino == usuario);
            }

            if (soloNoLeidas)
            {
                query = query.Where(n => !n.Leida);
            }

            if (idTicket.HasValue)
            {
                query = query.Where(n => n.IdTicket == idTicket.Value);
            }

            var notificaciones = await query
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<NotificacionDto>>(notificaciones));
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

        [HttpPut("marcar-todas-leidas")]
        public async Task<IActionResult> MarcarTodasLeidas([FromQuery] string idUsuarioDestino)
        {
            if (string.IsNullOrWhiteSpace(idUsuarioDestino))
            {
                ModelState.AddModelError(nameof(idUsuarioDestino), "El usuario destino es requerido.");
                return ValidationProblem(ModelState);
            }

            var usuario = idUsuarioDestino.Trim();

            if (EsSolicitanteSinPrivilegios())
            {
                var idUsuarioActual = ObtenerIdUsuarioAutenticado();

                if (idUsuarioActual is null || usuario != idUsuarioActual)
                {
                    return Forbid();
                }
            }

            if (!await _context.Users.AnyAsync(u => u.Id == usuario && u.Activo))
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

        private async Task<bool> ReferenciasValidas(CrearNotificacionDto request)
        {
            var esValido = true;

            if (!await _context.Users.AnyAsync(u => u.Id == request.IdUsuarioDestino && u.Activo))
            {
                ModelState.AddModelError(nameof(request.IdUsuarioDestino), "El usuario destino no existe o esta inactivo.");
                esValido = false;
            }

            if (request.IdTicket.HasValue &&
                !await _context.Tickets.AnyAsync(t => t.IdTicket == request.IdTicket.Value && t.Activo))
            {
                ModelState.AddModelError(nameof(request.IdTicket), "El ticket indicado no existe o esta inactivo.");
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

        private IQueryable<Notificacion> AplicarFiltroSolicitante(IQueryable<Notificacion> query)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();

            if (User.IsInRole(AppRoles.Administrador) || User.IsInRole(AppRoles.ResolvedorTicket))
            {
                return query;
            }

            if (EsSolicitanteSinPrivilegios() && idUsuario is not null)
            {
                query = query.Where(n => n.IdUsuarioDestino == idUsuario);
            }
            else
            {
                query = query.Where(n => false);
            }

            return query;
        }

        private bool PuedeVerNotificacion(Notificacion notificacion)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();

            return !EsSolicitanteSinPrivilegios() ||
                (idUsuario is not null && notificacion.IdUsuarioDestino == idUsuario);
        }

        private bool EsSolicitanteSinPrivilegios()
        {
            return User.IsInRole(AppRoles.Solicitante) &&
                !User.IsInRole(AppRoles.Administrador) &&
                !User.IsInRole(AppRoles.ResolvedorTicket);
        }
    }
}
