using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.SubcategoriaTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class SubcategoriaTicketController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SubcategoriaTicketController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [Authorize(Roles = AppRoles.Administrador)]
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<SubcategoriaTicketDto>>> GetSubcategoriasTicket(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idCategoriaTicket = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

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

            var totalRegistros = await query.CountAsync();

            var subcategorias = await query
                .OrderBy(s => s.CategoriaTicket.Nombre)
                .ThenBy(s => s.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<SubcategoriaTicketDto>>(subcategorias));

            return Ok(response);
        }

        [HttpGet("select")]
        [Authorize(Roles = $"{AppRoles.Administrador},{AppRoles.ResolvedorTicket},{AppRoles.Solicitante}")]
        public async Task<ActionResult<IEnumerable<SubcategoriaTicketSelectDto>>> GetSubcategoriasTicketSelect(
            [FromQuery] int? idCategoriaTicket = null,
            [FromQuery] string? buscar = null,
            [FromQuery] bool incluirInactivos = false)
        {
            var query = _context.SubcategoriaTickets
                .Include(s => s.CategoriaTicket)
                .AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(s => s.Activo && s.CategoriaTicket.Activo);
            }

            if (idCategoriaTicket.HasValue)
            {
                query = query.Where(s => s.IdCategoriaTicket == idCategoriaTicket.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var filtro = buscar.Trim();
                query = query.Where(s => s.Nombre.Contains(filtro) || s.CategoriaTicket.Nombre.Contains(filtro));
            }

            var subcategorias = await query
                .OrderBy(s => s.CategoriaTicket.Nombre)
                .ThenBy(s => s.Nombre)
                .Select(s => new SubcategoriaTicketSelectDto
                {
                    IdSubcategoriaTicket = s.IdSubcategoriaTicket,
                    IdCategoriaTicket = s.IdCategoriaTicket,
                    NombreCategoriaTicket = s.CategoriaTicket.Nombre,
                    Nombre = s.Nombre
                })
                .ToListAsync();

            return Ok(subcategorias);
        }
        [Authorize(Roles = AppRoles.Administrador)]
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
        [Authorize(Roles = AppRoles.Administrador)]
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
        [Authorize(Roles = AppRoles.Administrador)]
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
        [Authorize(Roles = AppRoles.Administrador)]
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
