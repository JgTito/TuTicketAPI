using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.SubcategoriaTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Administrador)]
    public class SubcategoriaTicketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SubcategoriaTicketController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubcategoriaTicketDto>>> GetSubcategoriasTicket(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idCategoriaTicket = null)
        {
            var query = _context.SubcategoriaTickets
                .Include(s => s.CategoriaTicket)
                .AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(s => s.Activo);
            }

            if (idCategoriaTicket.HasValue)
            {
                query = query.Where(s => s.IdCategoriaTicket == idCategoriaTicket.Value);
            }

            var subcategorias = await query
                .OrderBy(s => s.CategoriaTicket.Nombre)
                .ThenBy(s => s.Nombre)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<SubcategoriaTicketDto>>(subcategorias));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubcategoriaTicketDto>> GetSubcategoriaTicket([FromRoute] int id)
        {
            var subcategoria = await _context.SubcategoriaTickets
                .Include(s => s.CategoriaTicket)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdSubcategoriaTicket == id);

            if (subcategoria is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<SubcategoriaTicketDto>(subcategoria));
        }

        [HttpPost]
        public async Task<ActionResult<SubcategoriaTicketDto>> CreateSubcategoriaTicket([FromBody] CrearSubcategoriaTicketDto request)
        {
            Normalizar(request);

            var categoriaExiste = await _context.CategoriaTickets
                .AnyAsync(c => c.IdCategoriaTicket == request.IdCategoriaTicket && c.Activo);

            if (!categoriaExiste)
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada no existe o esta inactiva.");
                return ValidationProblem(ModelState);
            }

            var existeNombre = await _context.SubcategoriaTickets
                .AnyAsync(s => s.IdCategoriaTicket == request.IdCategoriaTicket && s.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe una subcategoria con ese nombre para la categoria indicada.");
                return ValidationProblem(ModelState);
            }

            var subcategoria = _mapper.Map<SubcategoriaTicket>(request);

            _context.SubcategoriaTickets.Add(subcategoria);
            await _context.SaveChangesAsync();

            await _context.Entry(subcategoria)
                .Reference(s => s.CategoriaTicket)
                .LoadAsync();

            var response = _mapper.Map<SubcategoriaTicketDto>(subcategoria);

            return CreatedAtAction(nameof(GetSubcategoriaTicket), new { id = subcategoria.IdSubcategoriaTicket }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSubcategoriaTicket([FromRoute] int id, [FromBody] ActualizarSubcategoriaTicketDto request)
        {
            var subcategoria = await _context.SubcategoriaTickets.FindAsync(id);

            if (subcategoria is null)
            {
                return NotFound();
            }

            Normalizar(request);

            var categoriaExiste = await _context.CategoriaTickets
                .AnyAsync(c => c.IdCategoriaTicket == request.IdCategoriaTicket && c.Activo);

            if (!categoriaExiste)
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada no existe o esta inactiva.");
                return ValidationProblem(ModelState);
            }

            var existeNombre = await _context.SubcategoriaTickets
                .AnyAsync(s =>
                    s.IdSubcategoriaTicket != id &&
                    s.IdCategoriaTicket == request.IdCategoriaTicket &&
                    s.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe una subcategoria con ese nombre para la categoria indicada.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, subcategoria);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSubcategoriaTicket([FromRoute] int id)
        {
            var subcategoria = await _context.SubcategoriaTickets.FindAsync(id);

            if (subcategoria is null)
            {
                return NotFound();
            }

            if (!subcategoria.Activo)
            {
                return NoContent();
            }

            subcategoria.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearSubcategoriaTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }

        private static void Normalizar(ActualizarSubcategoriaTicketDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }
    }
}
