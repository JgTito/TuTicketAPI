namespace TuTicketAPI.Dtos.TipoRelacionTicket
{
    public class TipoRelacionTicketSelectDto
    {
        public int IdTipoRelacionTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
