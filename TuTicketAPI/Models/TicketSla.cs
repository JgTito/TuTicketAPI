namespace TuTicketAPI.Models
{
    public class TicketSla
    {
        public int IdTicketSla { get; set; }
        public int IdTicket { get; set; }
        public int IdSlaRegla { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaLimitePrimeraRespuesta { get; set; }
        public DateTime FechaLimiteResolucion { get; set; }
        public DateTime? FechaPrimeraRespuestaReal { get; set; }
        public DateTime? FechaResolucionReal { get; set; }
        public bool PrimeraRespuestaVencida { get; set; }
        public bool ResolucionVencida { get; set; }
        public bool Activo { get; set; } = true;

        public Ticket Ticket { get; set; } = null!;
        public SlaRegla SlaRegla { get; set; } = null!;
    }
}
