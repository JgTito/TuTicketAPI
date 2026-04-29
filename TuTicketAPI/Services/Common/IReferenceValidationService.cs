namespace TuTicketAPI.Services.Common
{
    public interface IReferenceValidationService
    {
        Task<bool> UsuarioActivoExiste(string? idUsuario);
        Task<bool> TicketActivoExiste(int idTicket);
        Task<bool> CategoriaActivaExiste(int idCategoriaTicket);
        Task<bool> EquipoSoporteActivoExiste(int idEquipoSoporte);
        Task<bool> EstadoTicketActivoExiste(int idEstadoTicket);
        Task<bool> PrioridadActivaExiste(int idPrioridadTicket);
        Task<bool> SlaPoliticaActivaExiste(int idSlaPolitica);
        Task<bool> SubcategoriaActivaExiste(int idSubcategoriaTicket);
        Task<bool> TipoRelacionTicketActivoExiste(int idTipoRelacionTicket);
    }
}
