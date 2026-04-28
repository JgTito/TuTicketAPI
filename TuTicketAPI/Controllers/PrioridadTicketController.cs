using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.PrioridadTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrioridadTicketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PrioridadTicketController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrioridadTicketDto>>> GetPrioridadesTicket([FromQuery] bool incluirInactivos = false)
        {
            var query = _context.PrioridadTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }

            var prioridades = await query
                .OrderBy(p => p.Nivel)
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<PrioridadTicketDto>>(prioridades));
        }

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
