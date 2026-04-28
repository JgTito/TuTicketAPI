namespace TuTicketAPI.Models
{
    public class TicketRelacion
    {
        public int IdTicketRelacion { get; set; }
        public int IdTicketOrigen { get; set; }
        public int IdTicketRelacionado { get; set; }
        public int IdTipoRelacionTicket { get; set; }
        public string? Observacion { get; set; }
        public string IdUsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; } = true;

        public Ticket TicketOrigen { get; set; } = null!;
        public Ticket TicketRelacionado { get; set; } = null!;
        public TipoRelacionTicket TipoRelacionTicket { get; set; } = null!;
        public ApplicationUser UsuarioCreacion { get; set; } = null!;
    }
}
