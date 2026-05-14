using TuTicketAPI.Dtos.InformeIaSoporte;

namespace TuTicketAPI.Services.Informes
{
    public interface IInformeIaGeneracionService
    {
        Task<InformeIaGeneradoDto> GenerarInformeMensualAsync(
            int? anio = null,
            int? mes = null,
            int limiteTicketsMuestra = 40,
            bool aplicarFiltroAcceso = true,
            CancellationToken cancellationToken = default);
    }
}
