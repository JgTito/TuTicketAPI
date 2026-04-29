using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.SlaPolitica;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Administrador)]
    public class SlaPoliticaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SlaPoliticaController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<SlaPoliticaDto>>> GetSlaPoliticas(
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

            var query = _context.SlaPoliticas.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(s => s.Activo);
            }

            var totalRegistros = await query.CountAsync();

            var politicas = await query
                .OrderBy(s => s.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = new ResultadoPaginadoDto<SlaPoliticaDto>
            {
                Pagina = pagina,
                TamanoPagina = tamanoPagina,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina),
                Datos = _mapper.Map<IEnumerable<SlaPoliticaDto>>(politicas)
            };

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SlaPoliticaDto>> GetSlaPolitica([FromRoute] int id)
        {
            var politica = await _context.SlaPoliticas
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdSlaPolitica == id);

            if (politica is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<SlaPoliticaDto>(politica));
        }

        [HttpPost]
        public async Task<ActionResult<SlaPoliticaDto>> CreateSlaPolitica([FromBody] CrearSlaPoliticaDto request)
        {
            Normalizar(request);

            var existeNombre = await _context.SlaPoliticas
                .AnyAsync(s => s.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe una politica SLA con ese nombre.");
                return ValidationProblem(ModelState);
            }

            var politica = _mapper.Map<SlaPolitica>(request);

            _context.SlaPoliticas.Add(politica);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<SlaPoliticaDto>(politica);

            return CreatedAtAction(nameof(GetSlaPolitica), new { id = politica.IdSlaPolitica }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSlaPolitica([FromRoute] int id, [FromBody] ActualizarSlaPoliticaDto request)
        {
            var politica = await _context.SlaPoliticas.FindAsync(id);

            if (politica is null)
            {
                return NotFound();
            }

            Normalizar(request);

            var existeNombre = await _context.SlaPoliticas
                .AnyAsync(s => s.IdSlaPolitica != id && s.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe una politica SLA con ese nombre.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, politica);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSlaPolitica([FromRoute] int id)
        {
            var politica = await _context.SlaPoliticas.FindAsync(id);

            if (politica is null)
            {
                return NotFound();
            }

            if (!politica.Activo)
            {
                return NoContent();
            }

            politica.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearSlaPoliticaDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }

        private static void Normalizar(ActualizarSlaPoliticaDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }
    }
}
