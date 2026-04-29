using TuTicketAPI.Models;

namespace TuTicketAPI.Services.Tickets
{
    public interface ITicketAttachmentService
    {
        IReadOnlyList<string> ValidarArchivos(IReadOnlyList<IFormFile>? archivos, bool requiereAlMenosUno);
        Task<List<TicketAdjunto>> GuardarAdjuntos(int idTicket, IEnumerable<IFormFile>? archivos, string idUsuarioSubida, ICollection<string> rutasGuardadas);
        void EliminarArchivosGuardados(IEnumerable<string> rutasGuardadas);
    }
}
