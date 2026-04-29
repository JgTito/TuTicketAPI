using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
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
        public async Task<ActionResult<ResultadoPaginadoDto<TicketAdjuntoDto>>> GetAdjuntosPorTicket(
            [FromRoute] int idTicket,
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

            var totalRegistros = await query.CountAsync();

            var adjuntos = await query
                .OrderByDescending(a => a.FechaSubida)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = new ResultadoPaginadoDto<TicketAdjuntoDto>
            {
                Pagina = pagina,
                TamanoPagina = tamanoPagina,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina),
                Datos = _mapper.Map<IEnumerable<TicketAdjuntoDto>>(adjuntos)
            };

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

            if (!await PuedeVerTicket(adjunto.IdTicket))
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

            if (!await PuedeVerTicket(idTicket))
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

            var directorioTicket = Path.Combine(_environment.ContentRootPath, "Uploads", "Tickets", idTicket.ToString());
            Directory.CreateDirectory(directorioTicket);

            var rutasGuardadas = new List<string>();
            var adjuntos = new List<TicketAdjunto>();

            try
            {
                foreach (var archivo in request.Archivos)
                {
                    var extension = Path.GetExtension(archivo.FileName);
                    var nombreGuardado = $"{Guid.NewGuid():N}{extension}";
                    var rutaFisica = Path.Combine(directorioTicket, nombreGuardado);

                    await using (var stream = System.IO.File.Create(rutaFisica))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    rutasGuardadas.Add(rutaFisica);

                    adjuntos.Add(new TicketAdjunto
                    {
                        IdTicket = idTicket,
                        NombreArchivoOriginal = Path.GetFileName(archivo.FileName),
                        NombreArchivoGuardado = nombreGuardado,
                        RutaArchivo = rutaFisica,
                        TipoContenido = string.IsNullOrWhiteSpace(archivo.ContentType) ? "application/octet-stream" : archivo.ContentType,
                        Extension = string.IsNullOrWhiteSpace(extension) ? null : extension,
                        PesoBytes = archivo.Length,
                        IdUsuarioSubida = request.IdUsuarioSubida,
                        Activo = true
                    });
                }

                _context.TicketAdjuntos.AddRange(adjuntos);
                await _context.SaveChangesAsync();
            }
            catch
            {
                EliminarArchivosGuardados(rutasGuardadas);
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

        private bool ArchivosValidos(IReadOnlyList<IFormFile> archivos)
        {
            if (archivos.Count == 0)
            {
                ModelState.AddModelError(nameof(CrearTicketAdjuntoDto.Archivos), "Debe adjuntar al menos un archivo.");
                return false;
            }

            for (var i = 0; i < archivos.Count; i++)
            {
                if (archivos[i].Length == 0)
                {
                    ModelState.AddModelError($"{nameof(CrearTicketAdjuntoDto.Archivos)}[{i}]", "El archivo esta vacio.");
                }
            }

            return ModelState.IsValid;
        }

        private static void EliminarArchivosGuardados(IEnumerable<string> rutasGuardadas)
        {
            foreach (var ruta in rutasGuardadas)
            {
                try
                {
                    if (System.IO.File.Exists(ruta))
                    {
                        System.IO.File.Delete(ruta);
                    }
                }
                catch
                {
                    // La limpieza de archivos no debe ocultar el error original del upload.
                }
            }
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
