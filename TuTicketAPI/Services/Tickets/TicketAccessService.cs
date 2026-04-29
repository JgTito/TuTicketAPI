using TuTicketAPI.Models;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Services.Common;

namespace TuTicketAPI.Services.Tickets
{
    public class TicketAccessService : ITicketAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public TicketAccessService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public IQueryable<Ticket> AplicarFiltroAcceso(IQueryable<Ticket> query)
        {
            var idUsuario = _currentUserService.IdUsuario;

            if (_currentUserService.EsAdministrador)
            {
                return query;
            }

            if (idUsuario is null)
            {
                return query.Where(t => false);
            }

            if (_currentUserService.EsSolicitanteSinPrivilegios)
            {
                return query.Where(t => t.IdUsuarioSolicitante == idUsuario);
            }

            if (_currentUserService.EsResolvedorSinAdministrador)
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
            var idUsuario = _currentUserService.IdUsuario;

            if (_currentUserService.EsAdministrador)
            {
                return query;
            }

            if (idUsuario is null)
            {
                return query.Where(s => false);
            }

            if (_currentUserService.EsSolicitanteSinPrivilegios)
            {
                return query.Where(s => s.Ticket.IdUsuarioSolicitante == idUsuario);
            }

            if (_currentUserService.EsResolvedorSinAdministrador)
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
    }
}
