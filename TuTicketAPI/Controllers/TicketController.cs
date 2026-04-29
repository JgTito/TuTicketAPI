using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Constants;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.Ticket;
using TuTicketAPI.Dtos.TicketHistorial;
using TuTicketAPI.Enums;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Tickets;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : ApiControllerBase
    {
       
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITicketAccessService _ticketAccessService;
        private readonly ITicketAttachmentService _ticketAttachmentService;
        private readonly ITicketHistoryService _ticketHistoryService;

        public TicketController(ApplicationDbContext context, IMapper mapper, ITicketAccessService ticketAccessService, ITicketAttachmentService ticketAttachmentService, ITicketHistoryService ticketHistoryService)
        {
            _context = context;
            _mapper = mapper;
            _ticketAccessService = ticketAccessService;
            _ticketAttachmentService = ticketAttachmentService;
            _ticketHistoryService = ticketHistoryService;
        }

        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<TicketDto>>> GetTickets(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idEstadoTicket = null,
            [FromQuery] int? idPrioridadTicket = null,
            [FromQuery] int? idSubcategoriaTicket = null,
            [FromQuery] string? idUsuarioSolicitante = null,
            [FromQuery] string? idUsuarioAsignado = null,
            [FromQuery] string? buscar = null,
            [FromQuery] DateTime? fechaCreacionDesde = null,
            [FromQuery] DateTime? fechaCreacionHasta = null,
            [FromQuery] DateTime? fechaActualizacionDesde = null,
            [FromQuery] DateTime? fechaActualizacionHasta = null,
            [FromQuery] DateTime? fechaPrimeraRespuestaDesde = null,
            [FromQuery] DateTime? fechaPrimeraRespuestaHasta = null,
            [FromQuery] DateTime? fechaResolucionDesde = null,
            [FromQuery] DateTime? fechaResolucionHasta = null,
            [FromQuery] DateTime? fechaCierreDesde = null,
            [FromQuery] DateTime? fechaCierreHasta = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            ValidarRangoFechas(fechaCreacionDesde, fechaCreacionHasta, nameof(fechaCreacionDesde), "creacion");
            ValidarRangoFechas(fechaActualizacionDesde, fechaActualizacionHasta, nameof(fechaActualizacionDesde), "actualizacion");
            ValidarRangoFechas(fechaPrimeraRespuestaDesde, fechaPrimeraRespuestaHasta, nameof(fechaPrimeraRespuestaDesde), "primera respuesta");
            ValidarRangoFechas(fechaResolucionDesde, fechaResolucionHasta, nameof(fechaResolucionDesde), "resolucion");
            ValidarRangoFechas(fechaCierreDesde, fechaCierreHasta, nameof(fechaCierreDesde), "cierre");

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var query = TicketsConReferencias().AsNoTracking();
            query = _ticketAccessService.AplicarFiltroAcceso(query);

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

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var filtro = buscar.Trim();
                query = query.Where(t =>
                    t.Titulo.Contains(filtro) ||
                    t.Descripcion.Contains(filtro));
            }

            if (fechaCreacionDesde.HasValue)
            {
                var desde = fechaCreacionDesde.Value;
                query = query.Where(t => t.FechaCreacion >= desde);
            }

            if (fechaCreacionHasta.HasValue)
            {
                var hasta = fechaCreacionHasta.Value;
                query = query.Where(t => t.FechaCreacion <= hasta);
            }

            if (fechaActualizacionDesde.HasValue)
            {
                var desde = fechaActualizacionDesde.Value;
                query = query.Where(t => t.FechaActualizacion.HasValue && t.FechaActualizacion.Value >= desde);
            }

            if (fechaActualizacionHasta.HasValue)
            {
                var hasta = fechaActualizacionHasta.Value;
                query = query.Where(t => t.FechaActualizacion.HasValue && t.FechaActualizacion.Value <= hasta);
            }

            if (fechaPrimeraRespuestaDesde.HasValue)
            {
                var desde = fechaPrimeraRespuestaDesde.Value;
                query = query.Where(t => t.FechaPrimeraRespuesta.HasValue && t.FechaPrimeraRespuesta.Value >= desde);
            }

            if (fechaPrimeraRespuestaHasta.HasValue)
            {
                var hasta = fechaPrimeraRespuestaHasta.Value;
                query = query.Where(t => t.FechaPrimeraRespuesta.HasValue && t.FechaPrimeraRespuesta.Value <= hasta);
            }

            if (fechaResolucionDesde.HasValue)
            {
                var desde = fechaResolucionDesde.Value;
                query = query.Where(t => t.FechaResolucion.HasValue && t.FechaResolucion.Value >= desde);
            }

            if (fechaResolucionHasta.HasValue)
            {
                var hasta = fechaResolucionHasta.Value;
                query = query.Where(t => t.FechaResolucion.HasValue && t.FechaResolucion.Value <= hasta);
            }

            if (fechaCierreDesde.HasValue)
            {
                var desde = fechaCierreDesde.Value;
                query = query.Where(t => t.FechaCierre.HasValue && t.FechaCierre.Value >= desde);
            }

            if (fechaCierreHasta.HasValue)
            {
                var hasta = fechaCierreHasta.Value;
                query = query.Where(t => t.FechaCierre.HasValue && t.FechaCierre.Value <= hasta);
            }

            var totalRegistros = await query.CountAsync();

            var tickets = await query
                .OrderByDescending(t => t.FechaCreacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<TicketDto>>(tickets));

            return Ok(response);
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

            if (!await _ticketAccessService.PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TicketDto>(ticket));
        }

        [HttpGet("{id:int}/historial")]
        public async Task<ActionResult<ResultadoPaginadoDto<TicketHistorialDto>>> GetHistorial(
            [FromRoute] int id,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            if (!await _ticketAccessService.PuedeVerTicket(id))
            {
                return Forbid();
            }

            var query = _context.TicketHistoriales
                .Include(h => h.UsuarioModificacion)
                .AsNoTracking()
                .Where(h => h.IdTicket == id);

            var totalRegistros = await query.CountAsync();

            var historial = await query
                .OrderByDescending(h => h.FechaModificacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<TicketHistorialDto>>(historial));

            return Ok(response);
        }

        [HttpGet("{id:int}/estados-disponibles")]
        public async Task<ActionResult<IEnumerable<EstadoDisponibleTicketDto>>> GetEstadosDisponibles([FromRoute] int id)
        {
            var ticket = await _context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTicket == id);

            if (ticket is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(id))
            {
                return Forbid();
            }

            var estados = await _context.FlujoEstadoTickets
                .AsNoTracking()
                .Where(f =>
                    f.Activo &&
                    f.IdEstadoOrigen == ticket.IdEstadoTicket &&
                    f.EstadoDestino.Activo)
                .OrderBy(f => f.EstadoDestino.Orden)
                .ThenBy(f => f.EstadoDestino.Nombre)
                .Select(f => new EstadoDisponibleTicketDto
                {
                    IdFlujoEstadoTicket = f.IdFlujoEstadoTicket,
                    IdEstadoTicket = f.IdEstadoDestino,
                    Nombre = f.EstadoDestino.Nombre,
                    Descripcion = f.EstadoDestino.Descripcion,
                    EsEstadoFinal = f.EstadoDestino.EsEstadoFinal,
                    Orden = f.EstadoDestino.Orden,
                    RequiereComentario = f.RequiereComentario
                })
                .ToListAsync();

            return Ok(estados);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(25_000_000)]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromForm] CrearTicketDto request)
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

            if (!ArchivosValidos(request.Archivos, requiereAlMenosUno: false))
            {
                return ValidationProblem(ModelState);
            }

            var idUsuarioResponsable = await ObtenerUsuarioResponsableCategoria(request.IdSubcategoriaTicket);

            if (idUsuarioResponsable is null)
            {
                ModelState.AddModelError(nameof(request.IdSubcategoriaTicket), "La categoria de la subcategoria indicada no tiene un responsable activo.");
                return ValidationProblem(ModelState);
            }

            var nombreSolicitante = await _ticketHistoryService.ObtenerNombreUsuario(idUsuarioSolicitante);
            var nombreResponsable = await _ticketHistoryService.ObtenerNombreUsuario(idUsuarioResponsable);

            var ticket = _mapper.Map<Ticket>(request);
            ticket.Codigo = await GenerarCodigo();
            ticket.IdEstadoTicket = (int)EstadoTicketEnum.Abierto;
            ticket.IdUsuarioSolicitante = idUsuarioSolicitante;
            ticket.IdUsuarioAsignado = idUsuarioResponsable;

            var rutasGuardadas = new List<string>();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                var adjuntos = await _ticketAttachmentService.GuardarAdjuntos(ticket.IdTicket, request.Archivos, idUsuarioSolicitante, rutasGuardadas);
                _context.TicketAdjuntos.AddRange(adjuntos);

                _context.TicketHistoriales.Add(_ticketHistoryService.CrearHistorial(
                    ticket.IdTicket,
                    "Creacion",
                    null,
                    ticket.Codigo,
                    idUsuarioSolicitante,
                    $"Ticket {ticket.Codigo} creado por {nombreSolicitante}. Estado inicial: {EstadoTicketNombres.Abierto}."));

                _context.TicketHistoriales.Add(_ticketHistoryService.CrearHistorial(
                    ticket.IdTicket,
                    "Usuario asignado",
                    null,
                    idUsuarioResponsable,
                    idUsuarioSolicitante,
                    $"Responsable de categoria asignado automaticamente: {nombreResponsable}."));

                _context.TicketHistoriales.Add(_ticketHistoryService.CrearHistorial(
                    ticket.IdTicket,
                    "Estado ticket",
                    EstadoTicketNombres.Abierto,
                    EstadoTicketNombres.PendienteDeDerivacion,
                    idUsuarioSolicitante,
                    $"Ticket recibido por {nombreResponsable}, responsable de categoria. Cambio automatico de estado desde {EstadoTicketNombres.Abierto} a {EstadoTicketNombres.PendienteDeDerivacion}."));

                ticket.IdEstadoTicket = (int)EstadoTicketEnum.PendienteDeDerivacion;
                ticket.FechaActualizacion = DateTime.Now;

                await CrearSlaActivo(ticket);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                _ticketAttachmentService.EliminarArchivosGuardados(rutasGuardadas);
                throw;
            }

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

            if (!await _ticketAccessService.PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            var nombreUsuarioModificacion = await _ticketHistoryService.ObtenerNombreUsuario(request.IdUsuarioModificacion);
            var prioridadAnterior = await _ticketHistoryService.ObtenerNombrePrioridad(ticket.IdPrioridadTicket);
            var prioridadNueva = await _ticketHistoryService.ObtenerNombrePrioridad(request.IdPrioridadTicket);
            var subcategoriaAnterior = await _ticketHistoryService.ObtenerNombreSubcategoria(ticket.IdSubcategoriaTicket);
            var subcategoriaNueva = await _ticketHistoryService.ObtenerNombreSubcategoria(request.IdSubcategoriaTicket);
            var usuarioAsignadoAnterior = await _ticketHistoryService.ObtenerNombreUsuarioOpcional(ticket.IdUsuarioAsignado);
            var usuarioAsignadoNuevo = await _ticketHistoryService.ObtenerNombreUsuarioOpcional(request.IdUsuarioAsignado);
            var activoAnterior = ticket.Activo ? "Activo" : "Inactivo";
            var activoNuevo = request.Activo ? "Activo" : "Inactivo";

            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Titulo", ticket.Titulo, request.Titulo, request.IdUsuarioModificacion, _ticketHistoryService.ConstruirComentarioCambio(nombreUsuarioModificacion, "actualizo el titulo del ticket", ticket.Titulo, request.Titulo, request.Comentario));
            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Descripcion", ticket.Descripcion, request.Descripcion, request.IdUsuarioModificacion, _ticketHistoryService.ConstruirComentarioCambio(nombreUsuarioModificacion, "actualizo la descripcion del ticket", "Descripcion anterior", "Descripcion nueva", request.Comentario));
            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Prioridad ticket", prioridadAnterior, prioridadNueva, request.IdUsuarioModificacion, _ticketHistoryService.ConstruirComentarioCambio(nombreUsuarioModificacion, "actualizo la prioridad del ticket", prioridadAnterior, prioridadNueva, request.Comentario));
            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Subcategoria ticket", subcategoriaAnterior, subcategoriaNueva, request.IdUsuarioModificacion, _ticketHistoryService.ConstruirComentarioCambio(nombreUsuarioModificacion, "actualizo la subcategoria del ticket", subcategoriaAnterior, subcategoriaNueva, request.Comentario));
            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Usuario asignado", usuarioAsignadoAnterior, usuarioAsignadoNuevo, request.IdUsuarioModificacion, _ticketHistoryService.ConstruirComentarioCambio(nombreUsuarioModificacion, "actualizo el usuario asignado del ticket", usuarioAsignadoAnterior, usuarioAsignadoNuevo, request.Comentario));
            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Activo", activoAnterior, activoNuevo, request.IdUsuarioModificacion, _ticketHistoryService.ConstruirComentarioCambio(nombreUsuarioModificacion, "actualizo el estado activo del ticket", activoAnterior, activoNuevo, request.Comentario));

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

            if (!await _ticketAccessService.PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            Normalizar(request);

            var idUsuarioModificacion = ObtenerIdUsuarioAutenticado();

            if (idUsuarioModificacion is null)
            {
                return Unauthorized();
            }

            if (!await UsuarioActivoExiste(idUsuarioModificacion, "UsuarioModificacion") ||
                !await UsuarioActivoExiste(request.IdUsuarioAsignado, nameof(request.IdUsuarioAsignado)))
            {
                return ValidationProblem(ModelState);
            }

            var nombreUsuarioModificacion = await _ticketHistoryService.ObtenerNombreUsuario(idUsuarioModificacion);
            var nombreAsignadoAnterior = string.IsNullOrWhiteSpace(ticket.IdUsuarioAsignado)
                ? "Sin usuario asignado"
                : await _ticketHistoryService.ObtenerNombreUsuario(ticket.IdUsuarioAsignado);
            var nombreAsignadoNuevo = await _ticketHistoryService.ObtenerNombreUsuario(request.IdUsuarioAsignado);
            var comentario = $"Ticket asignado por {nombreUsuarioModificacion}. Responsable anterior: {nombreAsignadoAnterior}. Nuevo responsable: {nombreAsignadoNuevo}.";

            if (!string.IsNullOrWhiteSpace(request.Comentario))
            {
                comentario = $"{comentario} Comentario: {request.Comentario}";
            }

            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Usuario asignado", ticket.IdUsuarioAsignado, request.IdUsuarioAsignado, idUsuarioModificacion, comentario);


            var comentarioEstado = $"El ticket cambio de estado de forma automatica por derivacion.";

            if (!string.IsNullOrWhiteSpace(request.Comentario))
            {
                comentarioEstado = $"{comentarioEstado} Comentario: {request.Comentario}";
            }

            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Cambio estado", ticket.IdEstadoTicket.ToString(), EstadoTicketEnum.Derivado.ToString(), idUsuarioModificacion, comentarioEstado);


            ticket.IdUsuarioAsignado = request.IdUsuarioAsignado;
            ticket.FechaActualizacion = DateTime.Now;
            ticket.IdEstadoTicket = (int)EstadoTicketEnum.Derivado;
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

            if (!await _ticketAccessService.PuedeVerTicket(ticket))
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

            var estadoAnterior = await _ticketHistoryService.ObtenerNombreEstado(ticket.IdEstadoTicket);
            var nombreUsuarioModificacion = await _ticketHistoryService.ObtenerNombreUsuario(request.IdUsuarioModificacion);

            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Estado ticket", estadoAnterior, estadoDestino.Nombre, request.IdUsuarioModificacion, _ticketHistoryService.ConstruirComentarioCambio(nombreUsuarioModificacion, "cambio el estado del ticket", estadoAnterior, estadoDestino.Nombre, request.Comentario));
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

            if (!await _ticketAccessService.PuedeVerTicket(ticket))
            {
                return Forbid();
            }

            Normalizar(request);

            if (!await UsuarioActivoExiste(request.IdUsuarioModificacion, nameof(request.IdUsuarioModificacion)))
            {
                return ValidationProblem(ModelState);
            }

            var prioridadDestino = await _context.PrioridadTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPrioridadTicket == request.IdPrioridadTicket && p.Activo);

            if (prioridadDestino is null)
            {
                ModelState.AddModelError(nameof(request.IdPrioridadTicket), "La prioridad indicada no existe o esta inactiva.");
                return ValidationProblem(ModelState);
            }

            var prioridadAnterior = await _ticketHistoryService.ObtenerNombrePrioridad(ticket.IdPrioridadTicket);
            var nombreUsuarioModificacion = await _ticketHistoryService.ObtenerNombreUsuario(request.IdUsuarioModificacion);

            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Prioridad ticket", prioridadAnterior, prioridadDestino.Nombre, request.IdUsuarioModificacion, _ticketHistoryService.ConstruirComentarioCambio(nombreUsuarioModificacion, "cambio la prioridad del ticket", prioridadAnterior, prioridadDestino.Nombre, request.Comentario));
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

            if (!await _ticketAccessService.PuedeVerTicket(ticket))
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

            var nombreUsuarioModificacion = await _ticketHistoryService.ObtenerNombreUsuario(idUsuarioModificacion);

            _ticketHistoryService.RegistrarCambio(_context.TicketHistoriales, ticket.IdTicket, "Activo", "Activo", "Inactivo", idUsuarioModificacion, $"{nombreUsuarioModificacion} desactivo el ticket. Valor anterior: Activo. Nuevo valor: Inactivo.");
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

        private void ValidarRangoFechas(DateTime? desde, DateTime? hasta, string campoDesde, string nombreFecha)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            {
                ModelState.AddModelError(campoDesde, $"La fecha desde de {nombreFecha} no puede ser mayor que la fecha hasta.");
            }
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

        private Task<EstadoTicket?> ObtenerEstadoActivoPorNombre(string nombre)
        {
            return _context.EstadoTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Nombre == nombre && e.Activo);
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

        private bool ArchivosValidos(IReadOnlyList<IFormFile>? archivos, bool requiereAlMenosUno)
        {
            var errores = _ticketAttachmentService.ValidarArchivos(archivos, requiereAlMenosUno);

            for (var i = 0; i < errores.Count; i++)
            {
                ModelState.AddModelError($"{nameof(CrearTicketDto.Archivos)}[{i}]", errores[i]);
            }

            return errores.Count == 0;
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
