using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.CategoriaResponsable;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Common;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Administrador)]
    public class CategoriaResponsableController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IReferenceValidationService _referenceValidationService;

        public CategoriaResponsableController(ApplicationDbContext context, IMapper mapper, IReferenceValidationService referenceValidationService)
        {
            _context = context;
            _mapper = mapper;
            _referenceValidationService = referenceValidationService;
        }

        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<CategoriaResponsableDto>>> GetCategoriaResponsables(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idCategoriaTicket = null,
            [FromQuery] string? idUsuarioResponsable = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            var query = _context.CategoriaResponsables
                .Include(r => r.CategoriaTicket)
                .Include(r => r.UsuarioResponsable)
                .AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(r => r.Activo);
            }

            if (idCategoriaTicket.HasValue)
            {
                query = query.Where(r => r.IdCategoriaTicket == idCategoriaTicket.Value);
            }

            if (!string.IsNullOrWhiteSpace(idUsuarioResponsable))
            {
                var usuario = idUsuarioResponsable.Trim();
                query = query.Where(r => r.IdUsuarioResponsable == usuario);
            }

            var totalRegistros = await query.CountAsync();

            var responsables = await query
                .OrderBy(r => r.CategoriaTicket.Nombre)
                .ThenBy(r => r.UsuarioResponsable.NombreCompleto)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<CategoriaResponsableDto>>(responsables));

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaResponsableDto>> GetCategoriaResponsable([FromRoute] int id)
        {
            var responsable = await _context.CategoriaResponsables
                .Include(r => r.CategoriaTicket)
                .Include(r => r.UsuarioResponsable)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdCategoriaResponsable == id);

            if (responsable is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CategoriaResponsableDto>(responsable));
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaResponsableDto>> CreateCategoriaResponsable([FromBody] CrearCategoriaResponsableDto request)
        {
            Normalizar(request);

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            if (request.Activo && await ExisteResponsableActivo(request.IdCategoriaTicket))
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada ya tiene un responsable activo.");
                return ValidationProblem(ModelState);
            }

            var responsable = _mapper.Map<CategoriaResponsable>(request);

            _context.CategoriaResponsables.Add(responsable);
            await _context.SaveChangesAsync();

            await CargarReferencias(responsable);

            var response = _mapper.Map<CategoriaResponsableDto>(responsable);

            return CreatedAtAction(nameof(GetCategoriaResponsable), new { id = responsable.IdCategoriaResponsable }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategoriaResponsable([FromRoute] int id, [FromBody] ActualizarCategoriaResponsableDto request)
        {
            var responsable = await _context.CategoriaResponsables.FindAsync(id);

            if (responsable is null)
            {
                return NotFound();
            }

            Normalizar(request);

            if (!await ReferenciasValidas(request))
            {
                return ValidationProblem(ModelState);
            }

            if (request.Activo && await ExisteResponsableActivo(request.IdCategoriaTicket, id))
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada ya tiene un responsable activo.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, responsable);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategoriaResponsable([FromRoute] int id)
        {
            var responsable = await _context.CategoriaResponsables.FindAsync(id);

            if (responsable is null)
            {
                return NotFound();
            }

            if (!responsable.Activo)
            {
                return NoContent();
            }

            responsable.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ReferenciasValidas(CrearCategoriaResponsableDto request)
        {
            var esValido = true;

            if (!await _referenceValidationService.CategoriaActivaExiste(request.IdCategoriaTicket))
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada no existe o esta inactiva.");
                esValido = false;
            }

            if (!await _referenceValidationService.UsuarioActivoExiste(request.IdUsuarioResponsable))
            {
                ModelState.AddModelError(nameof(request.IdUsuarioResponsable), "El usuario responsable indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private async Task<bool> ReferenciasValidas(ActualizarCategoriaResponsableDto request)
        {
            var esValido = true;

            if (!await _referenceValidationService.CategoriaActivaExiste(request.IdCategoriaTicket))
            {
                ModelState.AddModelError(nameof(request.IdCategoriaTicket), "La categoria indicada no existe o esta inactiva.");
                esValido = false;
            }

            if (!await _referenceValidationService.UsuarioActivoExiste(request.IdUsuarioResponsable))
            {
                ModelState.AddModelError(nameof(request.IdUsuarioResponsable), "El usuario responsable indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private Task<bool> ExisteResponsableActivo(int idCategoriaTicket, int? idCategoriaResponsableExcluir = null)
        {
            return _context.CategoriaResponsables.AnyAsync(r =>
                r.Activo &&
                r.IdCategoriaTicket == idCategoriaTicket &&
                (!idCategoriaResponsableExcluir.HasValue || r.IdCategoriaResponsable != idCategoriaResponsableExcluir.Value));
        }

        private async Task CargarReferencias(CategoriaResponsable responsable)
        {
            await _context.Entry(responsable).Reference(r => r.CategoriaTicket).LoadAsync();
            await _context.Entry(responsable).Reference(r => r.UsuarioResponsable).LoadAsync();
        }

        private static void Normalizar(CrearCategoriaResponsableDto request)
        {
            request.IdUsuarioResponsable = request.IdUsuarioResponsable.Trim();
        }

        private static void Normalizar(ActualizarCategoriaResponsableDto request)
        {
            request.IdUsuarioResponsable = request.IdUsuarioResponsable.Trim();
        }
    }
}
