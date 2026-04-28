namespace TuTicketAPI.Dtos.TipoRelacionTicket
{
    public class TipoRelacionTicketDto
    {
        public int IdTipoRelacionTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
