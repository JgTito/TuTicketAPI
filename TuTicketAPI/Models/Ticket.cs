namespace TuTicketAPI.Models
{
    public class Ticket
    {
        public int IdTicket { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int IdEstadoTicket { get; set; }
        public int IdPrioridadTicket { get; set; }
        public int IdSubcategoriaTicket { get; set; }
        public string IdUsuarioSolicitante { get; set; } = string.Empty;
        public string? IdUsuarioAsignado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaPrimeraRespuesta { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int CantidadReaperturas { get; set; }
        public EstadoTicket EstadoTicket { get; set; } = null!;
        public PrioridadTicket PrioridadTicket { get; set; } = null!;
        public SubcategoriaTicket SubcategoriaTicket { get; set; } = null!;
        public ApplicationUser UsuarioSolicitante { get; set; } = null!;
        public ApplicationUser? UsuarioAsignado { get; set; }
        public ICollection<TicketSla> Slas { get; set; } = new List<TicketSla>();
        public ICollection<TicketHistorial> Historiales { get; set; } = new List<TicketHistorial>();
        public ICollection<TicketBitacora> Bitacoras { get; set; } = new List<TicketBitacora>();
        public ICollection<TicketAdjunto> Adjuntos { get; set; } = new List<TicketAdjunto>();
        public ICollection<TicketRelacion> RelacionesOrigen { get; set; } = new List<TicketRelacion>();
        public ICollection<TicketRelacion> RelacionesDestino { get; set; } = new List<TicketRelacion>();
        public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    }
}
