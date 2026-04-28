namespace TuTicketAPI.Dtos.TicketSla
{
    public class TicketSlaDto
    {
        public int IdTicketSla { get; set; }
        public int IdTicket { get; set; }
        public string CodigoTicket { get; set; } = string.Empty;
        public int IdSlaRegla { get; set; }
        public string NombreSlaPolitica { get; set; } = string.Empty;
        public string NombrePrioridadTicket { get; set; } = string.Empty;
        public string? NombreCategoriaTicket { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaLimitePrimeraRespuesta { get; set; }
        public DateTime FechaLimiteResolucion { get; set; }
        public DateTime? FechaPrimeraRespuestaReal { get; set; }
        public DateTime? FechaResolucionReal { get; set; }
        public bool PrimeraRespuestaVencida { get; set; }
        public bool ResolucionVencida { get; set; }
        public bool Activo { get; set; }
    }
}
