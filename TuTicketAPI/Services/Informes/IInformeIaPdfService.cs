using TuTicketAPI.Dtos.InformeIaSoporte;

namespace TuTicketAPI.Services.Informes
{
    public interface IInformeIaPdfService
    {
        byte[] GenerarPdf(InformeIaGeneradoDto informe);
    }
}
