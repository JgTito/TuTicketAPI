namespace TuTicketAPI.Models
{
    public class TicketBitacora
    {
        public int IdTicketBitacora { get; set; }
        public int IdTicket { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public bool EsInterno { get; set; }
        public string IdUsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; } = true;

        public Ticket Ticket { get; set; } = null!;
        public ApplicationUser UsuarioCreacion { get; set; } = null!;
    }
}
