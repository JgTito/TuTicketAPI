namespace TuTicketAPI.Models
{
    public class TipoRelacionTicket
    {
        public int IdTipoRelacionTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        public ICollection<TicketRelacion> TicketRelaciones { get; set; } = new List<TicketRelacion>();
    }
}
