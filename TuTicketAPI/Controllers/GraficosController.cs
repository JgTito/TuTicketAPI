using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Graficos;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Tickets;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{AppRoles.Administrador},{AppRoles.ResolvedorTicket},{AppRoles.Solicitante}")]
    public class GraficosController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITicketAccessService _ticketAccessService;

        public GraficosController(ApplicationDbContext context, ITicketAccessService ticketAccessService)
        {
            _context = context;
            _ticketAccessService = ticketAccessService;
        }

        [HttpGet("resumen")]
        public async Task<ActionResult<TicketResumenGraficoDto>> GetResumen(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (!ValidarRangoFechas(fechaDesde, fechaHasta))
            {
                return ValidationProblem(ModelState);
            }

            var ticketsQuery = CrearTicketQuery(fechaDesde, fechaHasta);
            var slasQuery = CrearTicketSlaQuery(fechaDesde, fechaHasta);

            var response = new TicketResumenGraficoDto
            {
                TotalTickets = await ticketsQuery.CountAsync(),
                TicketsAbiertos = await ticketsQuery.CountAsync(t => !t.EstadoTicket.EsEstadoFinal),
                TicketsCerrados = await ticketsQuery.CountAsync(t => t.EstadoTicket.EsEstadoFinal),
                TicketsSinAsignar = await ticketsQuery.CountAsync(t => t.IdUsuarioAsignado == null || t.IdUsuarioAsignado == string.Empty),
                TicketsReabiertos = await ticketsQuery.CountAsync(t => t.CantidadReaperturas > 0),
                SlasPrimeraRespuestaVencidos = await slasQuery.CountAsync(s => s.PrimeraRespuestaVencida),
                SlasResolucionVencidos = await slasQuery.CountAsync(s => s.ResolucionVencida)
            };

            return Ok(response);
        }

        [HttpGet("tickets-por-estado")]
        public async Task<ActionResult<IEnumerable<GraficoConteoDto>>> GetTicketsPorEstado(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (!ValidarRangoFechas(fechaDesde, fechaHasta))
            {
                return ValidationProblem(ModelState);
            }

            var datos = await CrearTicketQuery(fechaDesde, fechaHasta)
                .GroupBy(t => new { t.IdEstadoTicket, t.EstadoTicket.Nombre, t.EstadoTicket.Orden })
                .OrderBy(g => g.Key.Orden)
                .ThenBy(g => g.Key.Nombre)
                .Select(g => new GraficoConteoDto
                {
                    Id = g.Key.IdEstadoTicket,
                    Etiqueta = g.Key.Nombre,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return Ok(datos);
        }

        [HttpGet("tickets-por-prioridad")]
        public async Task<ActionResult<IEnumerable<GraficoConteoDto>>> GetTicketsPorPrioridad(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (!ValidarRangoFechas(fechaDesde, fechaHasta))
            {
                return ValidationProblem(ModelState);
            }

            var datos = await CrearTicketQuery(fechaDesde, fechaHasta)
                .GroupBy(t => new { t.IdPrioridadTicket, t.PrioridadTicket.Nombre, t.PrioridadTicket.Nivel })
                .OrderBy(g => g.Key.Nivel)
                .ThenBy(g => g.Key.Nombre)
                .Select(g => new GraficoConteoDto
                {
                    Id = g.Key.IdPrioridadTicket,
                    Etiqueta = g.Key.Nombre,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return Ok(datos);
        }

        [HttpGet("tickets-por-categoria")]
        public async Task<ActionResult<IEnumerable<GraficoConteoDto>>> GetTicketsPorCategoria(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (!ValidarRangoFechas(fechaDesde, fechaHasta))
            {
                return ValidationProblem(ModelState);
            }

            var datos = await CrearTicketQuery(fechaDesde, fechaHasta)
                .GroupBy(t => new
                {
                    t.SubcategoriaTicket.IdCategoriaTicket,
                    NombreCategoria = t.SubcategoriaTicket.CategoriaTicket.Nombre
                })
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key.NombreCategoria)
                .Select(g => new GraficoConteoDto
                {
                    Id = g.Key.IdCategoriaTicket,
                    Etiqueta = g.Key.NombreCategoria,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return Ok(datos);
        }

        [HttpGet("tickets-creados-por-mes")]
        public async Task<ActionResult<IEnumerable<GraficoSerieTemporalDto>>> GetTicketsCreadosPorMes(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (!ValidarRangoFechas(fechaDesde, fechaHasta))
            {
                return ValidationProblem(ModelState);
            }

            var datos = await CrearTicketQuery(fechaDesde, fechaHasta)
                .GroupBy(t => new { t.FechaCreacion.Year, t.FechaCreacion.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new GraficoSerieTemporalDto
                {
                    Anio = g.Key.Year,
                    Mes = g.Key.Month,
                    Etiqueta = g.Key.Year + "-" + g.Key.Month,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return Ok(datos);
        }

        [HttpGet("sla-cumplimiento")]
        public async Task<ActionResult<SlaCumplimientoGraficoDto>> GetSlaCumplimiento(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (!ValidarRangoFechas(fechaDesde, fechaHasta))
            {
                return ValidationProblem(ModelState);
            }

            var query = CrearTicketSlaQuery(fechaDesde, fechaHasta);

            var response = new SlaCumplimientoGraficoDto
            {
                DentroDeSlaPrimeraRespuesta = await query.CountAsync(s => !s.PrimeraRespuestaVencida),
                VencidosPrimeraRespuesta = await query.CountAsync(s => s.PrimeraRespuestaVencida),
                DentroDeSlaResolucion = await query.CountAsync(s => !s.ResolucionVencida),
                VencidosResolucion = await query.CountAsync(s => s.ResolucionVencida)
            };

            return Ok(response);
        }

        [HttpGet("tickets-por-responsable")]
        public async Task<ActionResult<IEnumerable<GraficoConteoDto>>> GetTicketsPorResponsable(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (!ValidarRangoFechas(fechaDesde, fechaHasta))
            {
                return ValidationProblem(ModelState);
            }

            var datos = await CrearTicketQuery(fechaDesde, fechaHasta)
                .GroupBy(t => new
                {
                    IdUsuario = t.IdUsuarioAsignado ?? string.Empty,
                    NombreUsuario = t.UsuarioAsignado == null ? "Sin responsable" : t.UsuarioAsignado.NombreCompleto
                })
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key.NombreUsuario)
                .Select(g => new GraficoConteoDto
                {
                    Id = 0,
                    Etiqueta = g.Key.NombreUsuario,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return Ok(datos);
        }

        private IQueryable<Ticket> CrearTicketQuery(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var query = _context.Tickets
                .Include(t => t.EstadoTicket)
                .Include(t => t.PrioridadTicket)
                .Include(t => t.SubcategoriaTicket)
                    .ThenInclude(s => s.CategoriaTicket)
                .Include(t => t.UsuarioAsignado)
                .AsNoTracking();

            query = _ticketAccessService.AplicarFiltroAcceso(query);
            query = AplicarFiltrosBase(query, fechaDesde, fechaHasta);

            return query;
        }

        private IQueryable<TicketSla> CrearTicketSlaQuery(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var query = _context.TicketSlas
                .Include(s => s.Ticket)
                    .ThenInclude(t => t.SubcategoriaTicket)
                .AsNoTracking();

            query = _ticketAccessService.AplicarFiltroAcceso(query);

            
            if (fechaDesde.HasValue)
            {
                query = query.Where(s => s.Ticket.FechaCreacion >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(s => s.Ticket.FechaCreacion <= fechaHasta.Value);
            }

            return query;
        }

        private static IQueryable<Ticket> AplicarFiltrosBase(
            IQueryable<Ticket> query,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {

            if (fechaDesde.HasValue)
            {
                query = query.Where(t => t.FechaCreacion >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(t => t.FechaCreacion <= fechaHasta.Value);
            }

            return query;
        }

        private bool ValidarRangoFechas(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde.Value > fechaHasta.Value)
            {
                ModelState.AddModelError(nameof(fechaDesde), "La fecha desde no puede ser mayor que la fecha hasta.");
            }

            return ModelState.IsValid;
        }
    }
}
