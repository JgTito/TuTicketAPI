using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.EquipoSoporte;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipoSoporteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EquipoSoporteController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipoSoporteDto>>> GetEquiposSoporte([FromQuery] bool incluirInactivos = false)
        {
            var query = _context.EquipoSoportes.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(e => e.Activo);
            }

            var equipos = await query
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<EquipoSoporteDto>>(equipos));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EquipoSoporteDto>> GetEquipoSoporte([FromRoute] int id)
        {
            var equipo = await _context.EquipoSoportes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEquipoSoporte == id);

            if (equipo is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<EquipoSoporteDto>(equipo));
        }

        [HttpPost]
        public async Task<ActionResult<EquipoSoporteDto>> CreateEquipoSoporte([FromBody] CrearEquipoSoporteDto request)
        {
            Normalizar(request);

            var existeNombre = await _context.EquipoSoportes
                .AnyAsync(e => e.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe un equipo de soporte con ese nombre.");
                return ValidationProblem(ModelState);
            }

            var equipo = _mapper.Map<EquipoSoporte>(request);

            _context.EquipoSoportes.Add(equipo);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<EquipoSoporteDto>(equipo);

            return CreatedAtAction(nameof(GetEquipoSoporte), new { id = equipo.IdEquipoSoporte }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEquipoSoporte([FromRoute] int id, [FromBody] ActualizarEquipoSoporteDto request)
        {
            var equipo = await _context.EquipoSoportes.FindAsync(id);

            if (equipo is null)
            {
                return NotFound();
            }

            Normalizar(request);

            var existeNombre = await _context.EquipoSoportes
                .AnyAsync(e => e.IdEquipoSoporte != id && e.Nombre == request.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(nameof(request.Nombre), "Ya existe un equipo de soporte con ese nombre.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, equipo);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEquipoSoporte([FromRoute] int id)
        {
            var equipo = await _context.EquipoSoportes.FindAsync(id);

            if (equipo is null)
            {
                return NotFound();
            }

            if (!equipo.Activo)
            {
                return NoContent();
            }

            equipo.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearEquipoSoporteDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }

        private static void Normalizar(ActualizarEquipoSoporteDto request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        }
    }
}
