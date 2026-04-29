using System.Security.Claims;
using TuTicketAPI.Authorization;

namespace TuTicketAPI.Services.Common
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? IdUsuario => Usuario?.FindFirstValue(ClaimTypes.NameIdentifier);

        public bool EsAdministrador => Usuario?.IsInRole(AppRoles.Administrador) == true;

        public bool EsSolicitanteSinPrivilegios =>
            Usuario?.IsInRole(AppRoles.Solicitante) == true &&
            !EsAdministrador &&
            Usuario.IsInRole(AppRoles.ResolvedorTicket) == false;

        public bool EsResolvedorSinAdministrador =>
            Usuario?.IsInRole(AppRoles.ResolvedorTicket) == true &&
            !EsAdministrador;

        private ClaimsPrincipal? Usuario => _httpContextAccessor.HttpContext?.User;
    }
}
