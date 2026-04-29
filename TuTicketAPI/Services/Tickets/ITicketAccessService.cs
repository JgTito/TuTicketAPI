using TuTicketAPI.Models;

namespace TuTicketAPI.Services.Tickets
{
    public interface ITicketAccessService
    {
        IQueryable<Ticket> AplicarFiltroAcceso(IQueryable<Ticket> query);
        IQueryable<TicketSla> AplicarFiltroAcceso(IQueryable<TicketSla> query);
        Task<bool> PuedeVerTicket(int idTicket);
        Task<bool> PuedeVerTicket(Ticket ticket);
    }
}
