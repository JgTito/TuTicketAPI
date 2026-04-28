using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Ticket;
using TuTicketAPI.Dtos.TicketHistorial;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TicketController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idEstadoTicket = null,
            [FromQuery] int? idPrioridadTicket = null,
            [FromQuery] int? idSubcategoriaTicket = null,
            [FromQuery] string? idUsuarioSolicitante = null,
            [FromQuery] string? idUsuarioAsignado = null)
        {
            var query = TicketsConReferencias().AsNoTracking();
            query = AplicarFiltroAcceso(query);

            if (!incluirInactivos)
            {
                query = query.Where(t => t.Activo);
            }

            if (idEstadoTicket.HasValue)
            {
                query = query.Where(t => t.IdEstadoTicket == idEstadoTicket.Value);
            }

            if (idPrioridadTicket.HasValue)
            {
                query = query.Where(t => t.IdPrioridadTicket == idPrioridadTicket.Value);
            }

            if (idSubcategoriaTicket.HasValue)
            {
                query = query.Where(t => t.IdSubcategoriaTicket == idSubcategoriaTicket.Value);
            }

            if (!string.IsNullOrWhiteSpace(idUsuarioSolicitante))
            {
                var usuario = idUsuarioSolicitante.Trim();
                query = query.Where(t => t.IdUsuarioSolicitante == usuario);
            }

            if (!string.IsNullOrWhiteSpace(idUsuarioAsignado))
            {
                var usuario = idUsuarioAsignado.Trim();
                query = query.Where(t => t.IdUsuarioAsignado == usuario);
            }

            var tickets = await query
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TicketDto>>(tickets));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketDto>> GetTicket([FromRoute] int id)
        {
            var ticket = await TicketsConReferencias()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTicket == id);

            if (ticket is null)
            {
                return NotFound();
            }

            if (!PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TicketDto>(ticket));
        }

        [HttpGet("{id:int}/historial")]
        public async Task<ActionResult<IEnumerable<TicketHistorialDto>>> GetHistorial([FromRoute] int id)
        {
            if (!await PuedeVerTicket(id))
            {
                return Forbid();
            }

            var historial = await _context.TicketHistoriales
                .Include(h => h.UsuarioModificacion)
                .AsNoTracking()
                .Where(h => h.IdTicket == id)
                .OrderByDescending(h => h.FechaModificacion)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TicketHistorialDto>>(historial));
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CrearTicketDto request)
        {
            Normalizar(request);

            var idUsuarioSolicitante = ObtenerIdUsuarioAutenticado();

            if (idUsuarioSolicitante is null)
            {
                return Unauthorized();
            }

            if (!await UsuarioActivoExiste(idUsuarioSolicitante, "UsuarioSolicitante"))
            {
                return ValidationProblem(ModelState);
            }

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            var idUsuarioResponsable = await ObtenerUsuarioResponsableCategoria(request.IdSubcategoriaTicket);

            if (idUsuarioResponsable is null)
            {
                ModelState.AddModelError(nameof(request.IdSubcategoriaTicket), "La categoria de la subcategoria indicada no tiene un responsable activo.");
                return ValidationProblem(ModelState);
            }

            var ticket = _mapper.Map<Ticket>(request);
            ticket.Codigo = await GenerarCodigo();
            ticket.IdUsuarioSolicitante = idUsuarioSolicitante;
            ticket.IdUsuarioAsignado = idUsuarioResponsable;

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            _context.TicketHistoriales.Add(CrearHistorial(ticket.IdTicket, "Creacion", null, ticket.Codigo, idUsuarioSolicitante, "Ticket creado."));
            _context.TicketHistoriales.Add(CrearHistorial(ticket.IdTicket, "IdUsuarioAsignado", null, idUsuarioResponsable, idUsuarioSolicitante, "Asignacion automatica al responsable de categoria."));
            await CrearSlaActivo(ticket);
            await _context.SaveChangesAsync();

            await CargarReferencias(ticket);

            var response = _mapper.Map<TicketDto>(ticket);

            return CreatedAtAction(nameof(GetTicket), new { id = ticket.IdTicket }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTicket([FromRoute] int id, [FromBody] ActualizarTicketDto request)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return NotFound();
            }

            if (!PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            RegistrarCambio(ticket.IdTicket, "Titulo", ticket.Titulo, request.Titulo, request.IdUsuarioModificacion, request.Comentario);
            RegistrarCambio(ticket.IdTicket, "Descripcion", ticket.Descripcion, request.Descripcion, request.IdUsuarioModificacion, request.Comentario);
            RegistrarCambio(ticket.IdTicket, "IdPrioridadTicket", ticket.IdPrioridadTicket.ToString(), request.IdPrioridadTicket.ToString(), request.IdUsuarioModificacion, request.Comentario);
            RegistrarCambio(ticket.IdTicket, "IdSubcategoriaTicket", ticket.IdSubcategoriaTicket.ToString(), request.IdSubcategoriaTicket.ToString(), request.IdUsuarioModificacion, request.Comentario);
            RegistrarCambio(ticket.IdTicket, "IdUsuarioAsignado", ticket.IdUsuarioAsignado, request.IdUsuarioAsignado, request.IdUsuarioModificacion, request.Comentario);
            RegistrarCambio(ticket.IdTicket, "Activo", ticket.Activo.ToString(), request.Activo.ToString(), request.IdUsuarioModificacion, request.Comentario);

            ticket.Titulo = request.Titulo;
            ticket.Descripcion = request.Descripcion;
            ticket.IdPrioridadTicket = request.IdPrioridadTicket;
            ticket.IdSubcategoriaTicket = request.IdSubcategoriaTicket;
            ticket.IdUsuarioAsignado = request.IdUsuarioAsignado;
            ticket.Activo = request.Activo;
            ticket.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}/asignar")]
        public async Task<IActionResult> AsignarTicket([FromRoute] int id, [FromBody] AsignarTicketDto request)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return NotFound();
            }

            if (!PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await UsuarioActivoExiste(request.IdUsuarioAsignado, nameof(request.IdUsuarioAsignado)) ||
                !await UsuarioActivoExiste(request.IdUsuarioModificacion, nameof(request.IdUsuarioModificacion)))
            {
                return ValidationProblem(ModelState);
            }

            RegistrarCambio(ticket.IdTicket, "IdUsuarioAsignado", ticket.IdUsuarioAsignado, request.IdUsuarioAsignado, request.IdUsuarioModificacion, request.Comentario);
            ticket.IdUsuarioAsignado = request.IdUsuarioAsignado;
            ticket.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstado([FromRoute] int id, [FromBody] CambiarEstadoTicketDto request)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return NotFound();
            }

            if (!PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await UsuarioActivoExiste(request.IdUsuarioModificacion, nameof(request.IdUsuarioModificacion)))
            {
                return ValidationProblem(ModelState);
            }

            var estadoDestino = await _context.EstadoTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEstadoTicket == request.IdEstadoTicket && e.Activo);

            if (estadoDestino is null)
            {
                ModelState.AddModelError(nameof(request.IdEstadoTicket), "El estado indicado no existe o esta inactivo.");
                return ValidationProblem(ModelState);
            }

            if (ticket.IdEstadoTicket != request.IdEstadoTicket)
            {
                var flujo = await _context.FlujoEstadoTickets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f =>
                        f.IdEstadoOrigen == ticket.IdEstadoTicket &&
                        f.IdEstadoDestino == request.IdEstadoTicket &&
                        f.Activo);

                if (flujo is null)
                {
                    ModelState.AddModelError(nameof(request.IdEstadoTicket), "No existe un flujo activo para cambiar al estado indicado.");
                    return ValidationProblem(ModelState);
                }

                if (flujo.RequiereComentario && string.IsNullOrWhiteSpace(request.Comentario))
                {
                    ModelState.AddModelError(nameof(request.Comentario), "El cambio de estado requiere comentario.");
                    return ValidationProblem(ModelState);
                }
            }

            RegistrarCambio(ticket.IdTicket, "IdEstadoTicket", ticket.IdEstadoTicket.ToString(), request.IdEstadoTicket.ToString(), request.IdUsuarioModificacion, request.Comentario);
            ticket.IdEstadoTicket = request.IdEstadoTicket;
            ticket.FechaActualizacion = DateTime.Now;

            if (estadoDestino.EsEstadoFinal)
            {
                ticket.FechaResolucion ??= DateTime.Now;
                ticket.FechaCierre ??= DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}/cambiar-prioridad")]
        public async Task<IActionResult> CambiarPrioridad([FromRoute] int id, [FromBody] CambiarPrioridadTicketDto request)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return NotFound();
            }

            if (!PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await UsuarioActivoExiste(request.IdUsuarioModificacion, nameof(request.IdUsuarioModificacion)))
            {
                return ValidationProblem(ModelState);
            }

            if (!await _context.PrioridadTickets.AnyAsync(p => p.IdPrioridadTicket == request.IdPrioridadTicket && p.Activo))
            {
                ModelState.AddModelError(nameof(request.IdPrioridadTicket), "La prioridad indicada no existe o esta inactiva.");
                return ValidationProblem(ModelState);
            }

            RegistrarCambio(ticket.IdTicket, "IdPrioridadTicket", ticket.IdPrioridadTicket.ToString(), request.IdPrioridadTicket.ToString(), request.IdUsuarioModificacion, request.Comentario);
            ticket.IdPrioridadTicket = request.IdPrioridadTicket;
            ticket.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTicket([FromRoute] int id, [FromQuery] string idUsuarioModificacion)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return NotFound();
            }

            if (!PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            if (!await UsuarioActivoExiste(idUsuarioModificacion, nameof(idUsuarioModificacion)))
            {
                return ValidationProblem(ModelState);
            }

            if (!ticket.Activo)
            {
                return NoContent();
            }

            RegistrarCambio(ticket.IdTicket, "Activo", ticket.Activo.ToString(), false.ToString(), idUsuarioModificacion, "Ticket desactivado.");
            ticket.Activo = false;
            ticket.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<Ticket> TicketsConReferencias()
        {
            return _context.Tickets
                .Include(t => t.EstadoTicket)
                .Include(t => t.PrioridadTicket)
                .Include(t => t.SubcategoriaTicket)
                    .ThenInclude(s => s.CategoriaTicket)
                .Include(t => t.UsuarioSolicitante)
                .Include(t => t.UsuarioAsignado);
        }

        private async Task<bool> ReferenciasValidas(CrearTicketDto request)
        {
            var esValido = true;

            if (!await _context.EstadoTickets.AnyAsync(e => e.IdEstadoTicket == request.IdEstadoTicket && e.Activo))
            {
                ModelState.AddModelError(nameof(request.IdEstadoTicket), "El estado indicado no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _context.PrioridadTickets.AnyAsync(p => p.IdPrioridadTicket == request.IdPrioridadTicket && p.Activo))
            {
                ModelState.AddModelError(nameof(request.IdPrioridadTicket), "La prioridad indicada no existe o esta inactiva.");
                esValido = false;
            }

            if (!await _context.SubcategoriaTickets.AnyAsync(s => s.IdSubcategoriaTicket == request.IdSubcategoriaTicket && s.Activo && s.CategoriaTicket.Activo))
            {
                ModelState.AddModelError(nameof(request.IdSubcategoriaTicket), "La subcategoria indicada no existe o esta inactiva.");
                esValido = false;
            }

            return esValido;
        }

        private async Task<bool> ReferenciasValidas(ActualizarTicketDto request)
        {
            var esValido = true;

            if (!await _context.PrioridadTickets.AnyAsync(p => p.IdPrioridadTicket == request.IdPrioridadTicket && p.Activo))
            {
                ModelState.AddModelError(nameof(request.IdPrioridadTicket), "La prioridad indicada no existe o esta inactiva.");
                esValido = false;
            }

            if (!await _context.SubcategoriaTickets.AnyAsync(s => s.IdSubcategoriaTicket == request.IdSubcategoriaTicket && s.Activo && s.CategoriaTicket.Activo))
            {
                ModelState.AddModelError(nameof(request.IdSubcategoriaTicket), "La subcategoria indicada no existe o esta inactiva.");
                esValido = false;
            }

            if (!await UsuarioActivoExiste(request.IdUsuarioModificacion, nameof(request.IdUsuarioModificacion)))
            {
                esValido = false;
            }

            if (!string.IsNullOrWhiteSpace(request.IdUsuarioAsignado) &&
                !await UsuarioActivoExiste(request.IdUsuarioAsignado, nameof(request.IdUsuarioAsignado)))
            {
                esValido = false;
            }

            return esValido;
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

        private IQueryable<Ticket> AplicarFiltroAcceso(IQueryable<Ticket> query)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();

            if (User.IsInRole(AppRoles.Administrador))
            {
                return query;
            }

            if (idUsuario is null)
            {
                return query.Where(t => false);
            }

            if (EsSolicitanteSinPrivilegios())
            {
                query = query.Where(t => t.IdUsuarioSolicitante == idUsuario);
            }
            else if (EsResolvedorSinAdministrador())
            {
                query = query.Where(t =>
                    t.IdUsuarioAsignado == idUsuario ||
                    _context.EquipoSoporteUsuarios.Any(eu =>
                        eu.Activo &&
                        eu.IdUsuario == idUsuario &&
                        _context.CategoriaEquipoSoportes.Any(ce =>
                            ce.Activo &&
                            ce.IdEquipoSoporte == eu.IdEquipoSoporte &&
                            ce.IdCategoriaTicket == t.SubcategoriaTicket.IdCategoriaTicket)));
            }
            else
            {
                query = query.Where(t => false);
            }

            return query;
        }

        private async Task<bool> PuedeVerTicket(int idTicket)
        {
            if (User.IsInRole(AppRoles.Administrador))
            {
                return await _context.Tickets.AnyAsync(t => t.IdTicket == idTicket);
            }

            var idUsuario = ObtenerIdUsuarioAutenticado();

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

        private bool PuedeVerTicket(Ticket ticket)
        {
            var idUsuario = ObtenerIdUsuarioAutenticado();

            if (User.IsInRole(AppRoles.Administrador))
            {
                return true;
            }

            if (idUsuario is null)
            {
                return false;
            }

            if (EsSolicitanteSinPrivilegios())
            {
                return ticket.IdUsuarioSolicitante == idUsuario;
            }

            if (EsResolvedorSinAdministrador())
            {
                return ticket.IdUsuarioAsignado == idUsuario ||
                    _context.EquipoSoporteUsuarios.Any(eu =>
                        eu.Activo &&
                        eu.IdUsuario == idUsuario &&
                        _context.CategoriaEquipoSoportes.Any(ce =>
                            ce.Activo &&
                            ce.IdEquipoSoporte == eu.IdEquipoSoporte &&
                            ce.IdCategoriaTicket == ticket.SubcategoriaTicket.IdCategoriaTicket));
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

        private string? ObtenerIdUsuarioAutenticado()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<string?> ObtenerUsuarioResponsableCategoria(int idSubcategoriaTicket)
        {
            var idCategoriaTicket = await _context.SubcategoriaTickets
                .Where(s => s.IdSubcategoriaTicket == idSubcategoriaTicket)
                .Select(s => s.IdCategoriaTicket)
                .FirstOrDefaultAsync();

            if (idCategoriaTicket == 0)
            {
                return null;
            }

            return await _context.CategoriaResponsables
                .Where(r =>
                    r.Activo &&
                    r.IdCategoriaTicket == idCategoriaTicket &&
                    r.UsuarioResponsable.Activo)
                .Select(r => r.IdUsuarioResponsable)
                .FirstOrDefaultAsync();
        }

        private async Task<string> GenerarCodigo()
        {
            string codigo;

            do
            {
                codigo = $"TCK-{DateTime.Now:yyyyMMddHHmmssfff}";
            }
            while (await _context.Tickets.AnyAsync(t => t.Codigo == codigo));

            return codigo;
        }

        private void RegistrarCambio(int idTicket, string campo, string? valorAnterior, string? valorNuevo, string idUsuarioModificacion, string? comentario)
        {
            if (valorAnterior == valorNuevo)
            {
                return;
            }

            _context.TicketHistoriales.Add(CrearHistorial(idTicket, campo, valorAnterior, valorNuevo, idUsuarioModificacion, comentario));
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

        private async Task CargarReferencias(Ticket ticket)
        {
            await _context.Entry(ticket).Reference(t => t.EstadoTicket).LoadAsync();
            await _context.Entry(ticket).Reference(t => t.PrioridadTicket).LoadAsync();
            await _context.Entry(ticket).Reference(t => t.SubcategoriaTicket).LoadAsync();
            await _context.Entry(ticket.SubcategoriaTicket).Reference(s => s.CategoriaTicket).LoadAsync();
            await _context.Entry(ticket).Reference(t => t.UsuarioSolicitante).LoadAsync();
            await _context.Entry(ticket).Reference(t => t.UsuarioAsignado).LoadAsync();
        }

        private async Task CrearSlaActivo(Ticket ticket)
        {
            var idCategoriaTicket = await _context.SubcategoriaTickets
                .Where(s => s.IdSubcategoriaTicket == ticket.IdSubcategoriaTicket)
                .Select(s => s.IdCategoriaTicket)
                .FirstAsync();

            var regla = await _context.SlaReglas
                .Where(s =>
                    s.Activo &&
                    s.IdPrioridadTicket == ticket.IdPrioridadTicket &&
                    s.IdCategoriaTicket == idCategoriaTicket)
                .OrderBy(s => s.IdSlaRegla)
                .FirstOrDefaultAsync();

            regla ??= await _context.SlaReglas
                .Where(s =>
                    s.Activo &&
                    s.IdPrioridadTicket == ticket.IdPrioridadTicket &&
                    s.IdCategoriaTicket == null)
                .OrderBy(s => s.IdSlaRegla)
                .FirstOrDefaultAsync();

            if (regla is null)
            {
                return;
            }

            var fechaInicio = DateTime.Now;

            _context.TicketSlas.Add(new TicketSla
            {
                IdTicket = ticket.IdTicket,
                IdSlaRegla = regla.IdSlaRegla,
                FechaInicio = fechaInicio,
                FechaLimitePrimeraRespuesta = fechaInicio.AddMinutes(regla.MinutosPrimeraRespuesta),
                FechaLimiteResolucion = fechaInicio.AddMinutes(regla.MinutosResolucion),
                PrimeraRespuestaVencida = false,
                ResolucionVencida = false,
                Activo = true
            });
        }

        private static void Normalizar(CrearTicketDto request)
        {
            request.Titulo = request.Titulo.Trim();
            request.Descripcion = request.Descripcion.Trim();
        }

        private static void Normalizar(ActualizarTicketDto request)
        {
            request.Titulo = request.Titulo.Trim();
            request.Descripcion = request.Descripcion.Trim();
            request.IdUsuarioAsignado = string.IsNullOrWhiteSpace(request.IdUsuarioAsignado) ? null : request.IdUsuarioAsignado.Trim();
            request.IdUsuarioModificacion = request.IdUsuarioModificacion.Trim();
            request.Comentario = string.IsNullOrWhiteSpace(request.Comentario) ? null : request.Comentario.Trim();
        }

        private static void Normalizar(AsignarTicketDto request)
        {
            request.IdUsuarioAsignado = request.IdUsuarioAsignado.Trim();
            request.IdUsuarioModificacion = request.IdUsuarioModificacion.Trim();
            request.Comentario = string.IsNullOrWhiteSpace(request.Comentario) ? null : request.Comentario.Trim();
        }

        private static void Normalizar(CambiarEstadoTicketDto request)
        {
            request.IdUsuarioModificacion = request.IdUsuarioModificacion.Trim();
            request.Comentario = string.IsNullOrWhiteSpace(request.Comentario) ? null : request.Comentario.Trim();
        }

        private static void Normalizar(CambiarPrioridadTicketDto request)
        {
            request.IdUsuarioModificacion = request.IdUsuarioModificacion.Trim();
            request.Comentario = string.IsNullOrWhiteSpace(request.Comentario) ? null : request.Comentario.Trim();
        }
    }
}
