using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.TicketBitacora;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketBitacoraController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TicketBitacoraController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("/api/Ticket/{idTicket:int}/bitacora")]
        public async Task<ActionResult<ResultadoPaginadoDto<TicketBitacoraDto>>> GetBitacorasPorTicket(
            [FromRoute] int idTicket,
            [FromQuery] bool incluirInactivos = false,
            [FromQuery] bool? esInterno = null,
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

            var query = _context.TicketBitacoras
                .Include(b => b.UsuarioCreacion)
                .AsNoTracking()
                .Where(b => b.IdTicket == idTicket);

            if (!incluirInactivos)
            {
                query = query.Where(b => b.Activo);
            }

            if (esInterno.HasValue)
            {
                query = query.Where(b => b.EsInterno == esInterno.Value);
            }

            var totalRegistros = await query.CountAsync();

            var bitacoras = await query
                .OrderByDescending(b => b.FechaCreacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = new ResultadoPaginadoDto<TicketBitacoraDto>
            {
                Pagina = pagina,
                TamanoPagina = tamanoPagina,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina),
                Datos = _mapper.Map<IEnumerable<TicketBitacoraDto>>(bitacoras)
            };

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketBitacoraDto>> GetTicketBitacora([FromRoute] int id)
        {
            var bitacora = await _context.TicketBitacoras
                .Include(b => b.UsuarioCreacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.IdTicketBitacora == id);

            if (bitacora is null)
            {
                return NotFound();
            }

            if (!await PuedeVerTicket(bitacora.IdTicket))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<TicketBitacoraDto>(bitacora));
        }

        [HttpPost("/api/Ticket/{idTicket:int}/bitacora")]
        public async Task<ActionResult<TicketBitacoraDto>> CreateTicketBitacora([FromRoute] int idTicket, [FromBody] CrearTicketBitacoraDto request)
        {
            Normalizar(request);

            if (!await PuedeVerTicket(idTicket))
            {
                return Forbid();
            }

            if (!await _context.Tickets.AnyAsync(t => t.IdTicket == idTicket && t.Activo))
            {
                ModelState.AddModelError(nameof(idTicket), "El ticket indicado no existe o esta inactivo.");
                return ValidationProblem(ModelState);
            }

            if (!await _context.Users.AnyAsync(u => u.Id == request.IdUsuarioCreacion && u.Activo))
            {
                ModelState.AddModelError(nameof(request.IdUsuarioCreacion), "El usuario indicado no existe o esta inactivo.");
                return ValidationProblem(ModelState);
            }

            var bitacora = _mapper.Map<TicketBitacora>(request);
            bitacora.IdTicket = idTicket;

            _context.TicketBitacoras.Add(bitacora);
            await _context.SaveChangesAsync();

            await _context.Entry(bitacora).Reference(b => b.UsuarioCreacion).LoadAsync();

            var response = _mapper.Map<TicketBitacoraDto>(bitacora);

            return CreatedAtAction(nameof(GetTicketBitacora), new { id = bitacora.IdTicketBitacora }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTicketBitacora([FromRoute] int id, [FromBody] ActualizarTicketBitacoraDto request)
        {
            var bitacora = await _context.TicketBitacoras.FindAsync(id);

            if (bitacora is null)
            {
                return NotFound();
            }

            if (!await PuedeVerTicket(bitacora.IdTicket))
            {
                return Forbid();
            }

            Normalizar(request);
            _mapper.Map(request, bitacora);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTicketBitacora([FromRoute] int id)
        {
            var bitacora = await _context.TicketBitacoras.FindAsync(id);

            if (bitacora is null)
            {
                return NotFound();
            }

            if (!await PuedeVerTicket(bitacora.IdTicket))
            {
                return Forbid();
            }

            if (!bitacora.Activo)
            {
                return NoContent();
            }

            bitacora.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static void Normalizar(CrearTicketBitacoraDto request)
        {
            request.Comentario = request.Comentario.Trim();
            request.IdUsuarioCreacion = request.IdUsuarioCreacion.Trim();
        }

        private static void Normalizar(ActualizarTicketBitacoraDto request)
        {
            request.Comentario = request.Comentario.Trim();
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
