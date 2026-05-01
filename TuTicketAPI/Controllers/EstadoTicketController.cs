using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.EstadoTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoTicketController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EstadoTicketController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [Authorize(Roles = AppRoles.Administrador)]
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<EstadoTicketDto>>> GetEstadosTicket(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            var query = _context.EstadoTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(e => e.Activo);
            }

            var totalRegistros = await query.CountAsync();

            var estados = await query
                .OrderBy(e => e.Orden)
                .ThenBy(e => e.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<EstadoTicketDto>>(estados));

            return Ok(response);
        }

        [Authorize(Roles = $"{AppRoles.Administrador},{AppRoles.ResolvedorTicket},{AppRoles.Solicitante}")]
        [HttpGet("select")]
        public async Task<ActionResult<IEnumerable<EstadoTicketSelectDto>>> GetEstadosTicketSelect(
            [FromQuery] string? buscar = null,
            [FromQuery] bool incluirInactivos = false)
        {
            var query = _context.EstadoTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(e => e.Activo);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var filtro = buscar.Trim();
                query = query.Where(e => e.Nombre.Contains(filtro));
            }

            var estados = await query
                .OrderBy(e => e.Orden)
                .ThenBy(e => e.Nombre)
                .Select(e => new EstadoTicketSelectDto
                {
                    IdEstadoTicket = e.IdEstadoTicket,
                    Nombre = e.Nombre,
                    EsEstadoFinal = e.EsEstadoFinal,
                    Orden = e.Orden
                })
                .ToListAsync();

            return Ok(estados);
        }

        [Authorize(Roles = AppRoles.Administrador)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EstadoTicketDto>> GetEstadoTicket([FromRoute] int id)
        {
            var estado = await _context.EstadoTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEstadoTicket == id);

            if (estado is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<EstadoTicketDto>(estado));
        }

        [Authorize(Roles = AppRoles.Administrador)]
        [HttpPost]
        public async Task<ActionResult<EstadoTicketDto>> CreateEstadoTicket([FromBody] CrearEstadoTicketDto request)
        {
            Normalizar(request);

            var existeNombre = await _context.EstadoTickets
                .AnyAsync(e => e.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe un estado de ticket con ese nombre.");
                return ValidationProblem(ModelState);
            }

            var estado = _mapper.Map<EstadoTicket>(request);

            _context.EstadoTickets.Add(estado);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<EstadoTicketDto>(estado);

            return CreatedAtAction(nameof(GetEstadoTicket), new { id = estado.IdEstadoTicket }, response);
        }

        [Authorize(Roles = AppRoles.Administrador)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEstadoTicket([FromRoute] int id, [FromBody] ActualizarEstadoTicketDto request)
        {
            var estado = await _context.EstadoTickets.FindAsync(id);

            if (estado is null)
            {
                return NotFound();
            }

            Normalizar(request);

            var existeNombre = await _context.EstadoTickets
                .AnyAsync(e => e.IdEstadoTicket != id && e.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe un estado de ticket con ese nombre.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, estado);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = AppRoles.Administrador)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEstadoTicket([FromRoute] int id)
        {
            var estado = await _context.EstadoTickets.FindAsync(id);

            if (estado is null)
            {
                return NotFound();
            }

            if (!estado.Activo)
            {
                return NoContent();
            }

            estado.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearEstadoTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }

        private static void Normalizar(ActualizarEstadoTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }
    }
}
