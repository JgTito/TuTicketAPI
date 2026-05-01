using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Models;

namespace TuTicketAPI.Services.Common
{
    public class ReferenceValidationService : IReferenceValidationService
    {
        private readonly ApplicationDbContext _context;

        public ReferenceValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<bool> UsuarioActivoExiste(string? idUsuario)
        {
            return !string.IsNullOrWhiteSpace(idUsuario)
                ? _context.Users.AnyAsync(u => u.Id == idUsuario && u.Activo)
                : Task.FromResult(false);
        }

        public Task<bool> TicketExiste(int idTicket)
        {
            return _context.Tickets.AnyAsync(t => t.IdTicket == idTicket);
        }

        public Task<bool> CategoriaActivaExiste(int idCategoriaTicket)
        {
            return _context.CategoriaTickets.AnyAsync(c => c.IdCategoriaTicket == idCategoriaTicket && c.Activo);
        }

        public Task<bool> EquipoSoporteActivoExiste(int idEquipoSoporte)
        {
            return _context.EquipoSoportes.AnyAsync(e => e.IdEquipoSoporte == idEquipoSoporte && e.Activo);
        }

        public Task<bool> EstadoTicketActivoExiste(int idEstadoTicket)
        {
            return _context.EstadoTickets.AnyAsync(e => e.IdEstadoTicket == idEstadoTicket && e.Activo);
        }

        public Task<bool> PrioridadActivaExiste(int idPrioridadTicket)
        {
            return _context.PrioridadTickets.AnyAsync(p => p.IdPrioridadTicket == idPrioridadTicket && p.Activo);
        }

        public Task<bool> SlaPoliticaActivaExiste(int idSlaPolitica)
        {
            return _context.SlaPoliticas.AnyAsync(s => s.IdSlaPolitica == idSlaPolitica && s.Activo);
        }

        public Task<bool> SubcategoriaActivaExiste(int idSubcategoriaTicket)
        {
            return _context.SubcategoriaTickets.AnyAsync(s =>
                s.IdSubcategoriaTicket == idSubcategoriaTicket &&
                s.Activo &&
                s.CategoriaTicket.Activo);
        }

        public Task<bool> TipoRelacionTicketActivoExiste(int idTipoRelacionTicket)
        {
            return _context.TipoRelacionTickets.AnyAsync(t => t.IdTipoRelacionTicket == idTipoRelacionTicket && t.Activo);
        }
    }
}
