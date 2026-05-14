using TuTicketAPI.Dtos.InformeIaSoporte;

namespace TuTicketAPI.Services.Informes
{
    public interface IInformeIaSoporteService
    {
        Task<InformeIaSoporteMensualDto> CrearContextoMensualAsync(
            int? anio = null,
            int? mes = null,
            int limiteTicketsMuestra = 40,
            bool aplicarFiltroAcceso = true,
            CancellationToken cancellationToken = default);
    }
}
