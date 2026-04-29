using System.Security.Claims;
using TuTicketAPI.Authorization;
using TuTicketAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TuTicketAPI.Services.Tickets
{
    public class TicketAccessService : ITicketAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TicketAccessService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IQueryable<Ticket> AplicarFiltroAcceso(IQueryable<Ticket> query)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var idUsuario = ObtenerIdUsuarioAutenticado();

            if (user?.IsInRole(AppRoles.Administrador) == true)
            {
                return query;
            }

            if (idUsuario is null)
            {
                return query.Where(t => false);
            }

            if (EsSolicitanteSinPrivilegios())
            {
                return query.Where(t => t.IdUsuarioSolicitante == idUsuario);
            }

            if (EsResolvedorSinAdministrador())
            {
                return query.Where(t =>
                    t.IdUsuarioAsignado == idUsuario ||
                    _context.EquipoSoporteUsuarios.Any(eu =>
                        eu.Activo &&
                        eu.IdUsuario == idUsuario &&
                        _context.CategoriaEquipoSoportes.Any(ce =>
                            ce.Activo &&
                            ce.IdEquipoSoporte == eu.IdEquipoSoporte &&
                            ce.IdCategoriaTicket == t.SubcategoriaTicket.IdCategoriaTicket)));
            }

            return query.Where(t => false);
        }

        public IQueryable<TicketSla> AplicarFiltroAcceso(IQueryable<TicketSla> query)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var idUsuario = ObtenerIdUsuarioAutenticado();

            if (user?.IsInRole(AppRoles.Administrador) == true)
            {
                return query;
            }

            if (idUsuario is null)
            {
                return query.Where(s => false);
            }

            if (EsSolicitanteSinPrivilegios())
            {
                return query.Where(s => s.Ticket.IdUsuarioSolicitante == idUsuario);
            }

            if (EsResolvedorSinAdministrador())
            {
                return query.Where(s =>
                    s.Ticket.IdUsuarioAsignado == idUsuario ||
                    _context.EquipoSoporteUsuarios.Any(eu =>
                        eu.Activo &&
                        eu.IdUsuario == idUsuario &&
                        _context.CategoriaEquipoSoportes.Any(ce =>
                            ce.Activo &&
                            ce.IdEquipoSoporte == eu.IdEquipoSoporte &&
                            ce.IdCategoriaTicket == s.Ticket.SubcategoriaTicket.IdCategoriaTicket)));
            }

            return query.Where(s => false);
        }

        public Task<bool> PuedeVerTicket(int idTicket)
        {
            var query = _context.Tickets.Where(t => t.IdTicket == idTicket);
            return AplicarFiltroAcceso(query).AnyAsync();
        }

        public Task<bool> PuedeVerTicket(Ticket ticket)
        {
            return PuedeVerTicket(ticket.IdTicket);
        }

        private string? ObtenerIdUsuarioAutenticado()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private bool EsSolicitanteSinPrivilegios()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.IsInRole(AppRoles.Solicitante) == true &&
                user.IsInRole(AppRoles.Administrador) == false &&
                user.IsInRole(AppRoles.ResolvedorTicket) == false;
        }

        private bool EsResolvedorSinAdministrador()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.IsInRole(AppRoles.ResolvedorTicket) == true &&
                user.IsInRole(AppRoles.Administrador) == false;
        }
    }
}
