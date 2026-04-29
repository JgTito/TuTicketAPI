using TuTicketAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TuTicketAPI.Services.Tickets
{
    public interface ITicketHistoryService
    {
        TicketHistorial CrearHistorial(int idTicket, string campo, string? valorAnterior, string? valorNuevo, string idUsuarioModificacion, string? comentario);
        void RegistrarCambio(DbSet<TicketHistorial> historiales, int idTicket, string campo, string? valorAnterior, string? valorNuevo, string idUsuarioModificacion, string? comentario);
        Task<string> ObtenerNombreUsuario(string idUsuario);
        Task<string> ObtenerNombreUsuarioOpcional(string? idUsuario);
        Task<string> ObtenerNombrePrioridad(int idPrioridadTicket);
        Task<string> ObtenerNombreEstado(int idEstadoTicket);
        Task<string> ObtenerNombreSubcategoria(int idSubcategoriaTicket);
        string ConstruirComentarioCambio(string nombreUsuarioModificacion, string accion, string? valorAnterior, string? valorNuevo, string? comentarioUsuario);
    }
}
