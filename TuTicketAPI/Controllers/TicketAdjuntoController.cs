using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.TicketAdjunto;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketAdjuntoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public TicketAdjuntoController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }

        [HttpGet("/api/Ticket/{idTicket:int}/adjuntos")]
        public async Task<ActionResult<IEnumerable<TicketAdjuntoDto>>> GetAdjuntosPorTicket(
            [FromRoute] int idTicket,
            [FromQuery] bool incluirInactivos = false)
        {
            if (!await PuedeVerTicket(idTicket))
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

            var adjuntos = await query
                .OrderByDescending(a => a.FechaSubida)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TicketAdjuntoDto>>(adjuntos));
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

            if (!await PuedeVerTicket(adjunto.IdTicket))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TicketAdjuntoDto>(adjunto));
        }

        [HttpPost("/api/Ticket/{idTicket:int}/adjuntos")]
        [RequestSizeLimit(25_000_000)]
        public async Task<ActionResult<TicketAdjuntoDto>> UploadAdjunto([FromRoute] int idTicket, [FromForm] CrearTicketAdjuntoDto request)
        {
            Normalizar(request);

            if (!await PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            if (!await ReferenciasValidas(idTicket, request))
            {
                return ValidationProblem(ModelState);
            }

            if (request.Archivo.Length == 0)
            {
                ModelState.AddModelError(nameof(request.Archivo), "El archivo esta vacio.");
                return ValidationProblem(ModelState);
            }

            var extension = Path.GetExtension(request.Archivo.FileName);
            var nombreGuardado = $"{Guid.NewGuid():N}{extension}";
            var directorioTicket = Path.Combine(_environment.ContentRootPath, "Uploads", "Tickets", idTicket.ToString());

            Directory.CreateDirectory(directorioTicket);

            var rutaFisica = Path.Combine(directorioTicket, nombreGuardado);

            await using (var stream = System.IO.File.Create(rutaFisica))
            {
                await request.Archivo.CopyToAsync(stream);
            }

            var adjunto = new TicketAdjunto
            {
                IdTicket = idTicket,
                NombreArchivoOriginal = Path.GetFileName(request.Archivo.FileName),
                NombreArchivoGuardado = nombreGuardado,
                RutaArchivo = rutaFisica,
                TipoContenido = string.IsNullOrWhiteSpace(request.Archivo.ContentType) ? "application/octet-stream" : request.Archivo.ContentType,
                Extension = string.IsNullOrWhiteSpace(extension) ? null : extension,
                PesoBytes = request.Archivo.Length,
                IdUsuarioSubida = request.IdUsuarioSubida,
                Activo = true
            };

            _context.TicketAdjuntos.Add(adjunto);
            await _context.SaveChangesAsync();

            await _context.Entry(adjunto).Reference(a => a.Ticket).LoadAsync();
            await _context.Entry(adjunto).Reference(a => a.UsuarioSubida).LoadAsync();

            var response = _mapper.Map<TicketAdjuntoDto>(adjunto);

            return CreatedAtAction(nameof(GetTicketAdjunto), new { id = adjunto.IdTicketAdjunto }, response);
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

            if (!await PuedeVerTicket(adjunto.IdTicket))
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

            if (!await PuedeVerTicket(adjunto.IdTicket))
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

        private async Task<bool> PuedeVerTicket(int idTicket)
        {
            if (User.IsInRole(AppRoles.Administrador))
            {
                return await _context.Tickets.AnyAsync(t => t.IdTicket == idTicket);
            }

            var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (idUsuario is null)
            {
                return false;
            }

            if (EsSolicitanteSinPrivilegios())
            {
                return await _context.Tickets.AnyAsync(t => t.IdTicket == idTicket && t.IdUsuarioSolicitante == idUsuario);
            }

            if (EsResolvedorSinAdministrador())
            {
                return await _context.Tickets.AnyAsync(t =>
                    t.IdTicket == idTicket &&
                    (t.IdUsuarioAsignado == idUsuario ||
                        _context.EquipoSoporteUsuarios.Any(eu =>
                            eu.Activo &&
                            eu.IdUsuario == idUsuario &&
                            _context.CategoriaEquipoSoportes.Any(ce =>
                                ce.Activo &&
                                ce.IdEquipoSoporte == eu.IdEquipoSoporte &&
                                ce.IdCategoriaTicket == t.SubcategoriaTicket.IdCategoriaTicket))));
            }

            return false;
        }

        private bool EsSolicitanteSinPrivilegios()
        {
            return User.IsInRole(AppRoles.Solicitante) &&
                !User.IsInRole(AppRoles.Administrador) &&
                !User.IsInRole(AppRoles.ResolvedorTicket);
        }

        private bool EsResolvedorSinAdministrador()
        {
            return User.IsInRole(AppRoles.ResolvedorTicket) &&
                !User.IsInRole(AppRoles.Administrador);
        }
    }
}
