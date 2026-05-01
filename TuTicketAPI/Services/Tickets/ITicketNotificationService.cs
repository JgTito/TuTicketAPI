using TuTicketAPI.Models;

namespace TuTicketAPI.Services.Tickets
{
    public interface ITicketNotificationService
    {
        Task NotificarCreacionTicket(Ticket ticket, string idUsuarioActor);
        Task NotificarActualizacionTicket(Ticket ticket, string idUsuarioActor, string? comentario);
        Task NotificarAsignacionTicket(Ticket ticket, string idUsuarioActor, string idUsuarioAsignado, string? comentario);
        Task NotificarCambioEstadoTicket(Ticket ticket, string idUsuarioActor, string estadoAnterior, string estadoNuevo, string? comentario);
        Task NotificarCambioPrioridadTicket(Ticket ticket, string idUsuarioActor, string prioridadAnterior, string prioridadNueva, string? comentario);
        Task NotificarComentarioTicket(int idTicket, string idUsuarioActor, bool esInterno);
        Task NotificarAdjuntosTicket(int idTicket, string idUsuarioActor, int cantidadAdjuntos);
    }
}
