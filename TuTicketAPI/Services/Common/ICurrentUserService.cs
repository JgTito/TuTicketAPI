namespace TuTicketAPI.Services.Common
{
    public interface ICurrentUserService
    {
        string? IdUsuario { get; }
        bool EsAdministrador { get; }
        bool EsSolicitanteSinPrivilegios { get; }
        bool EsResolvedorSinAdministrador { get; }
    }
}
