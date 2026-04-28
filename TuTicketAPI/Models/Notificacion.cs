namespace TuTicketAPI.Models
{
    public class Notificacion
    {
        public int IdNotificacion { get; set; }
        public string IdUsuarioDestino { get; set; } = string.Empty;
        public int? IdTicket { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLectura { get; set; }

        public ApplicationUser UsuarioDestino { get; set; } = null!;
        public Ticket? Ticket { get; set; }
    }
}
