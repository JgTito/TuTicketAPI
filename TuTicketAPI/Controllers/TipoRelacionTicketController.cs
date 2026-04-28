using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.TipoRelacionTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoRelacionTicketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TipoRelacionTicketController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoRelacionTicketDto>>> GetTiposRelacionTicket([FromQuery] bool incluirInactivos = false)
        {
            var query = _context.TipoRelacionTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(t => t.Activo);
            }

            var tiposRelacion = await query
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TipoRelacionTicketDto>>(tiposRelacion));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoRelacionTicketDto>> GetTipoRelacionTicket([FromRoute] int id)
        {
            var tipoRelacion = await _context.TipoRelacionTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTipoRelacionTicket == id);

            if (tipoRelacion is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TipoRelacionTicketDto>(tipoRelacion));
        }

        [HttpPost]
        public async Task<ActionResult<TipoRelacionTicketDto>> CreateTipoRelacionTicket([FromBody] CrearTipoRelacionTicketDto request)
        {
            Normalizar(request);

            var existeNombre = await _context.TipoRelacionTickets
                .AnyAsync(t => t.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe un tipo de relacion de ticket con ese nombre.");
                return ValidationProblem(ModelState);
            }

            var tipoRelacion = _mapper.Map<TipoRelacionTicket>(request);

            _context.TipoRelacionTickets.Add(tipoRelacion);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<TipoRelacionTicketDto>(tipoRelacion);

            return CreatedAtAction(nameof(GetTipoRelacionTicket), new { id = tipoRelacion.IdTipoRelacionTicket }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTipoRelacionTicket([FromRoute] int id, [FromBody] ActualizarTipoRelacionTicketDto request)
        {
            var tipoRelacion = await _context.TipoRelacionTickets.FindAsync(id);

            if (tipoRelacion is null)
            {
                return NotFound();
            }

            Normalizar(request);

            var existeNombre = await _context.TipoRelacionTickets
                .AnyAsync(t => t.IdTipoRelacionTicket != id && t.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe un tipo de relacion de ticket con ese nombre.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, tipoRelacion);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTipoRelacionTicket([FromRoute] int id)
        {
            var tipoRelacion = await _context.TipoRelacionTickets.FindAsync(id);

            if (tipoRelacion is null)
            {
                return NotFound();
            }

            if (!tipoRelacion.Activo)
            {
                return NoContent();
            }

            tipoRelacion.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearTipoRelacionTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }

        private static void Normalizar(ActualizarTipoRelacionTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }
    }
}
