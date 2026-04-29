using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.PrioridadTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class PrioridadTicketController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PrioridadTicketController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [Authorize(Roles = AppRoles.Administrador)]
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<PrioridadTicketDto>>> GetPrioridadesTicket(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            var query = _context.PrioridadTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }

            var totalRegistros = await query.CountAsync();

            var prioridades = await query
                .OrderBy(p => p.Nivel)
                .ThenBy(p => p.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<PrioridadTicketDto>>(prioridades));

            return Ok(response);
        }
        [Authorize(Roles = $"{AppRoles.Administrador},{AppRoles.ResolvedorTicket},{AppRoles.Solicitante}")]
        [HttpGet("select")]
        public async Task<ActionResult<IEnumerable<PrioridadTicketSelectDto>>> GetPrioridadesTicketSelect(
            [FromQuery] string? buscar = null,
            [FromQuery] bool incluirInactivos = false)
        {
            var query = _context.PrioridadTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var filtro = buscar.Trim();
                query = query.Where(p => p.Nombre.Contains(filtro));
            }

            var prioridades = await query
                .OrderBy(p => p.Nivel)
                .ThenBy(p => p.Nombre)
                .Select(p => new PrioridadTicketSelectDto
                {
                    IdPrioridadTicket = p.IdPrioridadTicket,
                    Nombre = p.Nombre,
                    Nivel = p.Nivel
                })
                .ToListAsync();

            return Ok(prioridades);
        }
        [Authorize(Roles = AppRoles.Administrador)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PrioridadTicketDto>> GetPrioridadTicket([FromRoute] int id)
        {
            var prioridad = await _context.PrioridadTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPrioridadTicket == id);

            if (prioridad is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<PrioridadTicketDto>(prioridad));
        }
        [Authorize(Roles = AppRoles.Administrador)]
        [HttpPost]
        public async Task<ActionResult<PrioridadTicketDto>> CreatePrioridadTicket([FromBody] CrearPrioridadTicketDto request)
        {
            Normalizar(request);

            var existeNombre = await _context.PrioridadTickets
                .AnyAsync(p => p.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe una prioridad de ticket con ese nombre.");
                return ValidationProblem(ModelState);
            }

            var prioridad = _mapper.Map<PrioridadTicket>(request);

            _context.PrioridadTickets.Add(prioridad);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<PrioridadTicketDto>(prioridad);

            return CreatedAtAction(nameof(GetPrioridadTicket), new { id = prioridad.IdPrioridadTicket }, response);
        }
        [Authorize(Roles = AppRoles.Administrador)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePrioridadTicket([FromRoute] int id, [FromBody] ActualizarPrioridadTicketDto request)
        {
            var prioridad = await _context.PrioridadTickets.FindAsync(id);

            if (prioridad is null)
            {
                return NotFound();
            }

            Normalizar(request);

            var existeNombre = await _context.PrioridadTickets
                .AnyAsync(p => p.IdPrioridadTicket != id && p.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe una prioridad de ticket con ese nombre.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, prioridad);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [Authorize(Roles = AppRoles.Administrador)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePrioridadTicket([FromRoute] int id)
        {
            var prioridad = await _context.PrioridadTickets.FindAsync(id);

            if (prioridad is null)
            {
                return NotFound();
            }

            if (!prioridad.Activo)
            {
                return NoContent();
            }

            prioridad.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearPrioridadTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }

        private static void Normalizar(ActualizarPrioridadTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }
    }
}
