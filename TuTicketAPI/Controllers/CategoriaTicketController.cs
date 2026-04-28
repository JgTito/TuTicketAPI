using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.CategoriaTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaTicketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriaTicketController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaTicketDto>>> GetCategoriasTicket([FromQuery] bool incluirInactivos = false)
        {
            var query = _context.CategoriaTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(c => c.Activo);
            }

            var categorias = await query
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CategoriaTicketDto>>(categorias));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaTicketDto>> GetCategoriaTicket([FromRoute] int id)
        {
            var categoria = await _context.CategoriaTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoriaTicket == id);

            if (categoria is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CategoriaTicketDto>(categoria));
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaTicketDto>> CreateCategoriaTicket([FromBody] CrearCategoriaTicketDto request)
        {
            Normalizar(request);

            var existeNombre = await _context.CategoriaTickets
                .AnyAsync(c => c.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe una categoria de ticket con ese nombre.");
                return ValidationProblem(ModelState);
            }

            var categoria = _mapper.Map<CategoriaTicket>(request);

            _context.CategoriaTickets.Add(categoria);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<CategoriaTicketDto>(categoria);

            return CreatedAtAction(nameof(GetCategoriaTicket), new { id = categoria.IdCategoriaTicket }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategoriaTicket([FromRoute] int id, [FromBody] ActualizarCategoriaTicketDto request)
        {
            var categoria = await _context.CategoriaTickets.FindAsync(id);

            if (categoria is null)
            {
                return NotFound();
            }

            Normalizar(request);

            var existeNombre = await _context.CategoriaTickets
                .AnyAsync(c => c.IdCategoriaTicket != id && c.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe una categoria de ticket con ese nombre.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, categoria);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategoriaTicket([FromRoute] int id)
        {
            var categoria = await _context.CategoriaTickets.FindAsync(id);

            if (categoria is null)
            {
                return NotFound();
            }

            if (!categoria.Activo)
            {
                return NoContent();
            }

            categoria.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearCategoriaTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }

        private static void Normalizar(ActualizarCategoriaTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }
    }
}
