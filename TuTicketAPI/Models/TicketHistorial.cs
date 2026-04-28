namespace TuTicketAPI.Models
{
    public class TicketHistorial
    {
        public int IdTicketHistorial { get; set; }
        public int IdTicket { get; set; }
        public string CampoModificado { get; set; } = string.Empty;
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }
        public string? Comentario { get; set; }
        public string IdUsuarioModificacion { get; set; } = string.Empty;
        public DateTime FechaModificacion { get; set; }

        public Ticket Ticket { get; set; } = null!;
        public ApplicationUser UsuarioModificacion { get; set; } = null!;
    }
}
