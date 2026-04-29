using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.TicketAdjunto;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Tickets;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketAdjuntoController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ITicketAccessService _ticketAccessService;
        private readonly ITicketAttachmentService _ticketAttachmentService;

        public TicketAdjuntoController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment environment, ITicketAccessService ticketAccessService, ITicketAttachmentService ticketAttachmentService)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
            _ticketAccessService = ticketAccessService;
            _ticketAttachmentService = ticketAttachmentService;
        }

        [HttpGet("/api/Ticket/{idTicket:int}/adjuntos")]
        public async Task<ActionResult<ResultadoPaginadoDto<TicketAdjuntoDto>>> GetAdjuntosPorTicket(
            [FromRoute] int idTicket,
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 5)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            if (!await _ticketAccessService.PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            var query = AdjuntosConReferencias()
                .AsNoTracking()
                .Where(a => a.IdTicket == idTicket);

            if (!incluirInactivos)
            {
                query = query.Where(a => a.Activo);
            }

            var totalRegistros = await query.CountAsync();

            var adjuntos = await query
                .OrderByDescending(a => a.FechaSubida)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                _mapper.Map<IEnumerable<TicketAdjuntoDto>>(adjuntos));

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketAdjuntoDto>> GetTicketAdjunto([FromRoute] int id)
        {
            var adjunto = await AdjuntosConReferencias()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdTicketAdjunto == id);

            if (adjunto is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(adjunto.IdTicket))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TicketAdjuntoDto>(adjunto));
        }

        [HttpPost("/api/Ticket/{idTicket:int}/adjuntos")]
        [RequestSizeLimit(25_000_000)]
        public async Task<ActionResult<IEnumerable<TicketAdjuntoDto>>> UploadAdjunto([FromRoute] int idTicket, [FromForm] CrearTicketAdjuntoDto request)
        {
            Normalizar(request);

            if (!await _ticketAccessService.PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            if (!await ReferenciasValidas(idTicket, request))
            {
                return ValidationProblem(ModelState);
            }

            if (!ArchivosValidos(request.Archivos))
            {
                return ValidationProblem(ModelState);
            }

            var rutasGuardadas = new List<string>();
            List<TicketAdjunto> adjuntos;

            try
            {
                adjuntos = await _ticketAttachmentService.GuardarAdjuntos(idTicket, request.Archivos, request.IdUsuarioSubida, rutasGuardadas);
                _context.TicketAdjuntos.AddRange(adjuntos);
                await _context.SaveChangesAsync();
            }
            catch
            {
                _ticketAttachmentService.EliminarArchivosGuardados(rutasGuardadas);
                throw;
            }

            foreach (var adjunto in adjuntos)
            {
                await _context.Entry(adjunto).Reference(a => a.Ticket).LoadAsync();
                await _context.Entry(adjunto).Reference(a => a.UsuarioSubida).LoadAsync();
            }

            var response = _mapper.Map<IEnumerable<TicketAdjuntoDto>>(adjuntos);

            return CreatedAtAction(nameof(GetAdjuntosPorTicket), new { idTicket }, response);
        }

        [HttpGet("{id:int}/descargar")]
        public async Task<IActionResult> DescargarAdjunto([FromRoute] int id)
        {
            var adjunto = await _context.TicketAdjuntos
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdTicketAdjunto == id && a.Activo);

            if (adjunto is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(adjunto.IdTicket))
            {
                return Forbid();
            }

            var rutaFisica = Path.GetFullPath(adjunto.RutaArchivo);
            var raizUploads = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "Uploads"));

            if (!rutaFisica.StartsWith(raizUploads, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(rutaFisica))
            {
                return NotFound();
            }

            return PhysicalFile(
                rutaFisica,
                string.IsNullOrWhiteSpace(adjunto.TipoContenido) ? "application/octet-stream" : adjunto.TipoContenido,
                adjunto.NombreArchivoOriginal);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTicketAdjunto([FromRoute] int id)
        {
            var adjunto = await _context.TicketAdjuntos.FindAsync(id);

            if (adjunto is null)
            {
                return NotFound();
            }

            if (!await _ticketAccessService.PuedeVerTicket(adjunto.IdTicket))
            {
                return Forbid();
            }

            if (!adjunto.Activo)
            {
                return NoContent();
            }

            adjunto.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<TicketAdjunto> AdjuntosConReferencias()
        {
            return _context.TicketAdjuntos
                .Include(a => a.Ticket)
                .Include(a => a.UsuarioSubida);
        }

        private async Task<bool> ReferenciasValidas(int idTicket, CrearTicketAdjuntoDto request)
        {
            var esValido = true;

            if (!await _context.Tickets.AnyAsync(t => t.IdTicket == idTicket && t.Activo))
            {
                ModelState.AddModelError(nameof(idTicket), "El ticket indicado no existe o esta inactivo.");
                esValido = false;
            }

            if (!await _context.Users.AnyAsync(u => u.Id == request.IdUsuarioSubida && u.Activo))
            {
                ModelState.AddModelError(nameof(request.IdUsuarioSubida), "El usuario indicado no existe o esta inactivo.");
                esValido = false;
            }

            return esValido;
        }

        private static void Normalizar(CrearTicketAdjuntoDto request)
        {
            request.IdUsuarioSubida = request.IdUsuarioSubida.Trim();
        }

        private bool ArchivosValidos(IReadOnlyList<IFormFile> archivos)
        {
            var errores = _ticketAttachmentService.ValidarArchivos(archivos, requiereAlMenosUno: true);

            for (var i = 0; i < errores.Count; i++)
            {
                ModelState.AddModelError($"{nameof(CrearTicketAdjuntoDto.Archivos)}[{i}]", errores[i]);
            }

            return errores.Count == 0;
        }

    }
}
