using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.Usuario;
using TuTicketAPI.Models;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private const string RolSolicitante = "Solicitante";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public UsuarioController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthUsuarioDto>> Registrar([FromBody] RegistrarUsuarioDto request)
        {
            Normalizar(request);

            if (!await _roleManager.RoleExistsAsync(RolSolicitante))
            {
                ModelState.AddModelError(nameof(RolSolicitante), "El rol Solicitante no existe.");
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
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            var resultado = await _userManager.CreateAsync(usuario, request.Password);

            if (!resultado.Succeeded)
            {
                AgregarErroresIdentity(resultado);
                return ValidationProblem(ModelState);
            }

            var rolResultado = await _userManager.AddToRoleAsync(usuario, RolSolicitante);

            if (!rolResultado.Succeeded)
            {
                AgregarErroresIdentity(rolResultado);
                return ValidationProblem(ModelState);
            }

            return Ok(await CrearRespuestaAuth(usuario));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthUsuarioDto>> Login([FromBody] LoginUsuarioDto request)
        {
            Normalizar(request);

            var usuario = await _userManager.FindByEmailAsync(request.Usuario)
                ?? await _userManager.FindByNameAsync(request.Usuario);

            if (usuario is null || !usuario.Activo)
            {
                return Unauthorized("Credenciales invalidas.");
            }

            var passwordValido = await _userManager.CheckPasswordAsync(usuario, request.Password);

            if (!passwordValido)
            {
                return Unauthorized("Credenciales invalidas.");
            }

            return Ok(await CrearRespuestaAuth(usuario));
        }

        [HttpGet("select")]
        [Authorize(Roles = $"{AppRoles.Administrador},{AppRoles.ResolvedorTicket}")]
        public async Task<ActionResult<IEnumerable<UsuarioSelectDto>>> GetUsuariosSelect(
            [FromQuery] string? buscar = null,
            [FromQuery] bool incluirInactivos = false)
        {
            var query = _context.Users.AsNoTracking();

            if (!incluirInactivos)
            {
                query = query.Where(u => u.Activo);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var filtro = buscar.Trim();

                query = query.Where(u =>
                    u.NombreCompleto.Contains(filtro) ||
                    (u.Email != null && u.Email.Contains(filtro)) ||
                    (u.UserName != null && u.UserName.Contains(filtro)));
            }

            var usuarios = await query
                .OrderBy(u => u.NombreCompleto)
                .ThenBy(u => u.Email)
                .Select(u => new UsuarioSelectDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    UserName = u.UserName
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        private async Task<AuthUsuarioDto> CrearRespuestaAuth(ApplicationUser usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            var expira = DateTime.UtcNow.AddMinutes(ObtenerMinutosExpiracion());

            return new AuthUsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Roles = roles,
                Token = CrearToken(usuario, roles, expira),
                Expira = expira
            };
        }

        private string CrearToken(ApplicationUser usuario, IEnumerable<string> roles, DateTime expira)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key no esta configurado.");
            var jwtIssuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer no esta configurado.");
            var jwtAudience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience no esta configurado.");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, usuario.Id),
                new(ClaimTypes.Name, usuario.UserName ?? string.Empty),
                new(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new("nombreCompleto", usuario.NombreCompleto)
            };

            claims.AddRange(roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expira,
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int ObtenerMinutosExpiracion()
        {
            return int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var minutos) && minutos > 0
                ? minutos
                : 120;
        }

        private void AgregarErroresIdentity(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
        }

        private static void Normalizar(RegistrarUsuarioDto request)
        {
            request.NombreCompleto = request.NombreCompleto.Trim();
            request.Email = request.Email.Trim();
        }

        private static void Normalizar(LoginUsuarioDto request)
        {
            request.Usuario = request.Usuario.Trim();
        }
    }
}
