namespace TuTicketAPI.Dtos.Graficos
{
    public class TicketResumenGraficoDto
    {
        public int TotalTickets { get; set; }
        public int TicketsAbiertos { get; set; }
        public int TicketsCerrados { get; set; }
        public int TicketsSinAsignar { get; set; }
        public int TicketsReabiertos { get; set; }
        public int SlasPrimeraRespuestaVencidos { get; set; }
        public int SlasResolucionVencidos { get; set; }
    }
}
