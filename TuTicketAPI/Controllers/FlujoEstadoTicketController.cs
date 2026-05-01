using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.FlujoEstadoTicket;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Common;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Administrador)]
    public class FlujoEstadoTicketController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IReferenceValidationService _referenceValidationService;

        public FlujoEstadoTicketController(
            ApplicationDbContext context,
            IMapper mapper,
            IReferenceValidationService referenceValidationService)
        {
            _context = context;
            _mapper = mapper;
            _referenceValidationService = referenceValidationService;
        }

        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<FlujoEstadoTicketDto>>> GetFlujosEstadoTicket(
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int? idEstadoOrigen = null,
            [FromQuery] int? idEstadoDestino = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            var query = _context.FlujoEstadoTickets
                .Include(f => f.EstadoOrigen)
                .Include(f => f.EstadoDestino)
                .AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(f => f.Activo);
            }

            if (idEstadoOrigen.HasValue)
            {
                query = query.Where(f => f.IdEstadoOrigen == idEstadoOrigen.Value);
            }

            if (idEstadoDestino.HasValue)
            {
                query = query.Where(f => f.IdEstadoDestino == idEstadoDestino.Value);
            }

            var totalRegistros = await query.CountAsync();

            var flujos = await query
                .OrderBy(f => f.EstadoOrigen.Orden)
                .ThenBy(f => f.EstadoDestino.Orden)
                .ThenBy(f => f.EstadoOrigen.Nombre)
                .ThenBy(f => f.EstadoDestino.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<FlujoEstadoTicketDto>>(flujos));

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FlujoEstadoTicketDto>> GetFlujoEstadoTicket([FromRoute] int id)
        {
            var flujo = await _context.FlujoEstadoTickets
                .Include(f => f.EstadoOrigen)
                .Include(f => f.EstadoDestino)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.IdFlujoEstadoTicket == id);

            if (flujo is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<FlujoEstadoTicketDto>(flujo));
        }

        [HttpPost]
        public async Task<ActionResult<FlujoEstadoTicketDto>> CreateFlujoEstadoTicket([FromBody] CrearFlujoEstadoTicketDto request)
        {
            if (!await ValidarFlujo(request.IdEstadoOrigen, request.IdEstadoDestino))
            {
                return ValidationProblem(ModelState);
            }

            var existeFlujo = await _context.FlujoEstadoTickets
                .AnyAsync(f => f.IdEstadoOrigen == request.IdEstadoOrigen && f.IdEstadoDestino == request.IdEstadoDestino);

            if (existeFlujo)
            {
                ModelState.AddModelError(nameof(request.IdEstadoDestino), "Ya existe un flujo entre los estados indicados.");
                return ValidationProblem(ModelState);
            }

            var flujo = _mapper.Map<FlujoEstadoTicket>(request);

            _context.FlujoEstadoTickets.Add(flujo);
            await _context.SaveChangesAsync();

            await CargarReferencias(flujo);

            var response = _mapper.Map<FlujoEstadoTicketDto>(flujo);

            return CreatedAtAction(nameof(GetFlujoEstadoTicket), new { id = flujo.IdFlujoEstadoTicket }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFlujoEstadoTicket([FromRoute] int id, [FromBody] ActualizarFlujoEstadoTicketDto request)
        {
            var flujo = await _context.FlujoEstadoTickets.FindAsync(id);

            if (flujo is null)
            {
                return NotFound();
            }

            if (!await ValidarFlujo(request.IdEstadoOrigen, request.IdEstadoDestino))
            {
                return ValidationProblem(ModelState);
            }

            var existeFlujo = await _context.FlujoEstadoTickets
                .AnyAsync(f =>
                    f.IdFlujoEstadoTicket != id &&
                    f.IdEstadoOrigen == request.IdEstadoOrigen &&
                    f.IdEstadoDestino == request.IdEstadoDestino);

            if (existeFlujo)
            {
                ModelState.AddModelError(nameof(request.IdEstadoDestino), "Ya existe un flujo entre los estados indicados.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, flujo);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFlujoEstadoTicket([FromRoute] int id)
        {
            var flujo = await _context.FlujoEstadoTickets.FindAsync(id);

            if (flujo is null)
            {
                return NotFound();
            }

            if (!flujo.Activo)
            {
                return NoContent();
            }

            flujo.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ValidarFlujo(int idEstadoOrigen, int idEstadoDestino)
        {
            var esValido = true;

            if (idEstadoOrigen == idEstadoDestino)
            {
                ModelState.AddModelError(nameof(CrearFlujoEstadoTicketDto.IdEstadoDestino), "El estado origen y destino deben ser diferentes.");
                return false;
            }

            if (!await _referenceValidationService.EstadoTicketActivoExiste(idEstadoOrigen))
            {
                ModelState.AddModelError(nameof(CrearFlujoEstadoTicketDto.IdEstadoOrigen), "El estado origen indicado no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _referenceValidationService.EstadoTicketActivoExiste(idEstadoDestino))
            {
                ModelState.AddModelError(nameof(CrearFlujoEstadoTicketDto.IdEstadoDestino), "El estado destino indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private async Task CargarReferencias(FlujoEstadoTicket flujo)
        {
            await _context.Entry(flujo).Reference(f => f.EstadoOrigen).LoadAsync();
            await _context.Entry(flujo).Reference(f => f.EstadoDestino).LoadAsync();
        }
    }
}
