using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.EquipoSoporteUsuario;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Common;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Administrador)]
    public class EquipoSoporteUsuarioController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IReferenceValidationService _referenceValidationService;

        public EquipoSoporteUsuarioController(ApplicationDbContext context, IMapper mapper, IReferenceValidationService referenceValidationService)
        {
            _context = context;
            _mapper = mapper;
            _referenceValidationService = referenceValidationService;
        }

        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<EquipoSoporteUsuarioDto>>> GetEquipoSoporteUsuarios(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idEquipoSoporte = null,
            [FromQuery] string? idUsuario = null,
            [FromQuery] bool? esLider = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            var query = _context.EquipoSoporteUsuarios
                .Include(e => e.EquipoSoporte)
                .Include(e => e.Usuario)
                .AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(e => e.Activo);
            }

            if (idEquipoSoporte.HasValue)
            {
                query = query.Where(e => e.IdEquipoSoporte == idEquipoSoporte.Value);
            }

            if (!string.IsNullOrWhiteSpace(idUsuario))
            {
                var usuario = idUsuario.Trim();
                query = query.Where(e => e.IdUsuario == usuario);
            }

            if (esLider.HasValue)
            {
                query = query.Where(e => e.EsLider == esLider.Value);
            }

            var totalRegistros = await query.CountAsync();

            var usuarios = await query
                .OrderBy(e => e.EquipoSoporte.Nombre)
                .ThenByDescending(e => e.EsLider)
                .ThenBy(e => e.Usuario.NombreCompleto)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<EquipoSoporteUsuarioDto>>(usuarios));

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EquipoSoporteUsuarioDto>> GetEquipoSoporteUsuario([FromRoute] int id)
        {
            var usuarioEquipo = await _context.EquipoSoporteUsuarios
                .Include(e => e.EquipoSoporte)
                .Include(e => e.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEquipoSoporteUsuario == id);

            if (usuarioEquipo is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<EquipoSoporteUsuarioDto>(usuarioEquipo));
        }

        [HttpPost]
        public async Task<ActionResult<EquipoSoporteUsuarioDto>> CreateEquipoSoporteUsuario([FromBody] CrearEquipoSoporteUsuarioDto request)
        {
            Normalizar(request);

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            if (request.Activo && await ExisteUsuarioActivoEnEquipo(request.IdEquipoSoporte, request.IdUsuario))
            {
                ModelState.AddModelError(nameof(request.IdUsuario), "El usuario indicado ya esta activo en el equipo de soporte.");
                return ValidationProblem(ModelState);
            }

            var usuarioEquipo = _mapper.Map<EquipoSoporteUsuario>(request);

            _context.EquipoSoporteUsuarios.Add(usuarioEquipo);
            await _context.SaveChangesAsync();

            await CargarReferencias(usuarioEquipo);

            var response = _mapper.Map<EquipoSoporteUsuarioDto>(usuarioEquipo);

            return CreatedAtAction(nameof(GetEquipoSoporteUsuario), new { id = usuarioEquipo.IdEquipoSoporteUsuario }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEquipoSoporteUsuario([FromRoute] int id, [FromBody] ActualizarEquipoSoporteUsuarioDto request)
        {
            var usuarioEquipo = await _context.EquipoSoporteUsuarios.FindAsync(id);

            if (usuarioEquipo is null)
            {
                return NotFound();
            }

            Normalizar(request);

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            if (request.Activo && await ExisteUsuarioActivoEnEquipo(request.IdEquipoSoporte, request.IdUsuario, id))
            {
                ModelState.AddModelError(nameof(request.IdUsuario), "El usuario indicado ya esta activo en el equipo de soporte.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, usuarioEquipo);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEquipoSoporteUsuario([FromRoute] int id)
        {
            var usuarioEquipo = await _context.EquipoSoporteUsuarios.FindAsync(id);

            if (usuarioEquipo is null)
            {
                return NotFound();
            }

            if (!usuarioEquipo.Activo)
            {
                return NoContent();
            }

            usuarioEquipo.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ReferenciasValidas(CrearEquipoSoporteUsuarioDto request)
        {
            var esValido = true;

            if (!await _referenceValidationService.EquipoSoporteActivoExiste(request.IdEquipoSoporte))
            {
                ModelState.AddModelError(nameof(request.IdEquipoSoporte), "El equipo de soporte indicado no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _referenceValidationService.UsuarioActivoExiste(request.IdUsuario))
            {
                ModelState.AddModelError(nameof(request.IdUsuario), "El usuario indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private async Task<bool> ReferenciasValidas(ActualizarEquipoSoporteUsuarioDto request)
        {
            var esValido = true;

            if (!await _referenceValidationService.EquipoSoporteActivoExiste(request.IdEquipoSoporte))
            {
                ModelState.AddModelError(nameof(request.IdEquipoSoporte), "El equipo de soporte indicado no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _referenceValidationService.UsuarioActivoExiste(request.IdUsuario))
            {
                ModelState.AddModelError(nameof(request.IdUsuario), "El usuario indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private Task<bool> ExisteUsuarioActivoEnEquipo(int idEquipoSoporte, string idUsuario, int? idEquipoSoporteUsuarioExcluir = null)
        {
            return _context.EquipoSoporteUsuarios.AnyAsync(e =>
                e.Activo &&
                e.IdEquipoSoporte == idEquipoSoporte &&
                e.IdUsuario == idUsuario &&
                (!idEquipoSoporteUsuarioExcluir.HasValue || e.IdEquipoSoporteUsuario != idEquipoSoporteUsuarioExcluir.Value));
        }

        private async Task CargarReferencias(EquipoSoporteUsuario usuarioEquipo)
        {
            await _context.Entry(usuarioEquipo).Reference(e => e.EquipoSoporte).LoadAsync();
            await _context.Entry(usuarioEquipo).Reference(e => e.Usuario).LoadAsync();
        }

        private static void Normalizar(CrearEquipoSoporteUsuarioDto request)
        {
            request.IdUsuario = request.IdUsuario.Trim();
        }

        private static void Normalizar(ActualizarEquipoSoporteUsuarioDto request)
        {
            request.IdUsuario = request.IdUsuario.Trim();
        }
    }
}
