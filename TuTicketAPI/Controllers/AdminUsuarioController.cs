using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Comun;
using TuTicketAPI.Dtos.Usuario;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Common;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Administrador)]
    public class AdminUsuarioController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;

        public AdminUsuarioController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<AdminUsuarioDto>>> GetUsuarios(
            [FromQuery] string? buscar = null,
            [FromQuery] bool? activo = null,
            [FromQuery] string? rol = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 10)
        {
            var errorPaginacion = ValidarPaginacion(pagina, tamanoPagina);
            if (errorPaginacion is not null)
            {
                return errorPaginacion;
            }

            var query = _context.Users.AsNoTracking();

            if (activo.HasValue)
            {
                query = query.Where(u => u.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var filtro = buscar.Trim();

                query = query.Where(u =>
                    u.NombreCompleto.Contains(filtro) ||
                    (u.Email != null && u.Email.Contains(filtro)) ||
                    (u.UserName != null && u.UserName.Contains(filtro)));
            }

            if (!string.IsNullOrWhiteSpace(rol))
            {
                var rolNormalizado = rol.Trim().ToUpperInvariant();

                query = query.Where(u =>
                    _context.UserRoles.Any(ur =>
                        ur.UserId == u.Id &&
                        _context.Roles.Any(r => r.Id == ur.RoleId && r.NormalizedName == rolNormalizado)));
            }

            var totalRegistros = await query.CountAsync();

            var usuarios = await query
                .OrderBy(u => u.NombreCompleto)
                .ThenBy(u => u.Email)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var response = CrearResultadoPaginado(
                pagina,
                tamanoPagina,
                totalRegistros,
                await MapearUsuarios(usuarios));

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUsuarioDto>> GetUsuario([FromRoute] string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return NotFound();
            }

            return Ok(await MapearUsuario(usuario));
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RolUsuarioDto>>> GetRoles()
        {
            var roles = await _roleManager.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new RolUsuarioDto
                {
                    Id = r.Id,
                    Nombre = r.Name ?? string.Empty
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpPost]
        public async Task<ActionResult<AdminUsuarioDto>> CreateUsuario([FromBody] CrearAdminUsuarioDto request)
        {
            Normalizar(request);

            if (!await RolesValidos(request.Roles))
            {
                return ValidationProblem(ModelState);
            }

            var existeEmail = await _userManager.FindByEmailAsync(request.Email);

            if (existeEmail is not null)
            {
                ModelState.AddModelError(nameof(request.Email), "Ya existe un usuario con ese email.");
                return ValidationProblem(ModelState);
            }

            var usuario = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NombreCompleto = request.NombreCompleto,
                Activo = request.Activo,
                FechaCreacion = DateTime.Now
            };

            var resultado = await _userManager.CreateAsync(usuario, request.Password);

            if (!resultado.Succeeded)
            {
                AgregarErroresIdentity(resultado);
                return ValidationProblem(ModelState);
            }

            var rolesResultado = await _userManager.AddToRolesAsync(usuario, request.Roles);

            if (!rolesResultado.Succeeded)
            {
                await _userManager.DeleteAsync(usuario);
                AgregarErroresIdentity(rolesResultado);
                return ValidationProblem(ModelState);
            }

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, await MapearUsuario(usuario));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario([FromRoute] string id, [FromBody] ActualizarAdminUsuarioDto request)
        {
            Normalizar(request);

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return NotFound();
            }

            if (!request.Activo && EsUsuarioActual(usuario.Id))
            {
                ModelState.AddModelError(nameof(request.Activo), "No puede desactivar su propia cuenta.");
                return ValidationProblem(ModelState);
            }

            var usuarioConEmail = await _userManager.FindByEmailAsync(request.Email);

            if (usuarioConEmail is not null && usuarioConEmail.Id != usuario.Id)
            {
                ModelState.AddModelError(nameof(request.Email), "Ya existe un usuario con ese email.");
                return ValidationProblem(ModelState);
            }

            usuario.NombreCompleto = request.NombreCompleto;
            usuario.Email = request.Email;
            usuario.UserName = request.Email;
            usuario.Activo = request.Activo;

            var resultado = await _userManager.UpdateAsync(usuario);

            if (!resultado.Succeeded)
            {
                AgregarErroresIdentity(resultado);
                return ValidationProblem(ModelState);
            }

            return NoContent();
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstadoUsuario([FromRoute] string id, [FromBody] ActualizarEstadoUsuarioDto request)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return NotFound();
            }

            if (!request.Activo && EsUsuarioActual(usuario.Id))
            {
                ModelState.AddModelError(nameof(request.Activo), "No puede desactivar su propia cuenta.");
                return ValidationProblem(ModelState);
            }

            usuario.Activo = request.Activo;

            var resultado = await _userManager.UpdateAsync(usuario);

            if (!resultado.Succeeded)
            {
                AgregarErroresIdentity(resultado);
                return ValidationProblem(ModelState);
            }

            return NoContent();
        }

        [HttpPut("{id}/roles")]
        public async Task<IActionResult> UpdateRolesUsuario([FromRoute] string id, [FromBody] ActualizarRolesUsuarioDto request)
        {
            Normalizar(request);

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return NotFound();
            }

            if (!await RolesValidos(request.Roles))
            {
                return ValidationProblem(ModelState);
            }

            if (EsUsuarioActual(usuario.Id) && !request.Roles.Contains(AppRoles.Administrador, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(request.Roles), "No puede quitarse el rol Administrador a su propia cuenta.");
                return ValidationProblem(ModelState);
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            var removerResultado = await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);

            if (!removerResultado.Succeeded)
            {
                AgregarErroresIdentity(removerResultado);
                return ValidationProblem(ModelState);
            }

            var agregarResultado = await _userManager.AddToRolesAsync(usuario, request.Roles);

            if (!agregarResultado.Succeeded)
            {
                AgregarErroresIdentity(agregarResultado);
                return ValidationProblem(ModelState);
            }

            return NoContent();
        }

        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword([FromRoute] string id, [FromBody] ResetPasswordUsuarioDto request)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return NotFound();
            }

            if (await _userManager.HasPasswordAsync(usuario))
            {
                var removerResultado = await _userManager.RemovePasswordAsync(usuario);

                if (!removerResultado.Succeeded)
                {
                    AgregarErroresIdentity(removerResultado);
                    return ValidationProblem(ModelState);
                }
            }

            var agregarResultado = await _userManager.AddPasswordAsync(usuario, request.NuevaPassword);

            if (!agregarResultado.Succeeded)
            {
                AgregarErroresIdentity(agregarResultado);
                return ValidationProblem(ModelState);
            }

            return NoContent();
        }

        private async Task<IEnumerable<AdminUsuarioDto>> MapearUsuarios(IEnumerable<ApplicationUser> usuarios)
        {
            var response = new List<AdminUsuarioDto>();

            foreach (var usuario in usuarios)
            {
                response.Add(await MapearUsuario(usuario));
            }

            return response;
        }

        private async Task<AdminUsuarioDto> MapearUsuario(ApplicationUser usuario)
        {
            return new AdminUsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion,
                Roles = await _userManager.GetRolesAsync(usuario)
            };
        }

        private async Task<bool> RolesValidos(IEnumerable<string> roles)
        {
            var esValido = true;

            foreach (var rol in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!await _roleManager.RoleExistsAsync(rol))
                {
                    ModelState.AddModelError(nameof(roles), $"El rol '{rol}' no existe.");
                    esValido = false;
                }
            }

            return esValido;
        }

        private bool EsUsuarioActual(string idUsuario)
        {
            return _currentUserService.IdUsuario == idUsuario;
        }

        private void AgregarErroresIdentity(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
        }

        private static void Normalizar(CrearAdminUsuarioDto request)
        {
            request.NombreCompleto = request.NombreCompleto.Trim();
            request.Email = request.Email.Trim();
            request.Roles = NormalizarRoles(request.Roles);
        }

        private static void Normalizar(ActualizarAdminUsuarioDto request)
        {
            request.NombreCompleto = request.NombreCompleto.Trim();
            request.Email = request.Email.Trim();
        }

        private static void Normalizar(ActualizarRolesUsuarioDto request)
        {
            request.Roles = NormalizarRoles(request.Roles);
        }

        private static List<string> NormalizarRoles(IEnumerable<string> roles)
        {
            return roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
