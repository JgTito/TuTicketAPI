using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.CategoriaTicket;
using TuTicketAPI.Dtos.Comun;
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
        [Authorize(Roles = AppRoles.Administrador)]
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<CategoriaTicketDto>>> GetCategoriasTicket(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 10)
        {
            if (pagina < 1)
            {
                ModelState.AddModelError(nameof(pagina), "La pagina debe ser mayor o igual a 1.");
                return ValidationProblem(ModelState);
            }

            if (tamanoPagina < 1 || tamanoPagina > 100)
            {
                ModelState.AddModelError(nameof(tamanoPagina), "El tamano de pagina debe estar entre 1 y 100.");
                return ValidationProblem(ModelState);
            }

            var query = _context.CategoriaTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(c => c.Activo);
            }

            var totalRegistros = await query.CountAsync();

            var categorias = await query
                .OrderBy(c => c.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = new ResultadoPaginadoDto<CategoriaTicketDto>
            {
                Pagina = pagina,
                TamanoPagina = tamanoPagina,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina),
                Datos = _mapper.Map<IEnumerable<CategoriaTicketDto>>(categorias)
            };

            return Ok(response);
        }
        [Authorize(Roles = $"{AppRoles.Administrador},{AppRoles.ResolvedorTicket},{AppRoles.Solicitante}")]
        [HttpGet("select")]
        public async Task<ActionResult<IEnumerable<CategoriaTicketSelectDto>>> GetCategoriasTicketSelect(
            [FromQuery] string? buscar = null,
            [FromQuery] bool incluirInactivos = false)
        {
            var query = _context.CategoriaTickets.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(c => c.Activo);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var filtro = buscar.Trim();
                query = query.Where(c => c.Nombre.Contains(filtro));
            }

            var categorias = await query
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaTicketSelectDto
                {
                    IdCategoriaTicket = c.IdCategoriaTicket,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            return Ok(categorias);
        }
        [Authorize(Roles = AppRoles.Administrador)]
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
        [Authorize(Roles = AppRoles.Administrador)]
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
        [Authorize(Roles = AppRoles.Administrador)]
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
        [Authorize(Roles = AppRoles.Administrador)]
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
