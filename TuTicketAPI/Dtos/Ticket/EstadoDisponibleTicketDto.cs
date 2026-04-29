namespace TuTicketAPI.Dtos.Ticket
{
    public class EstadoDisponibleTicketDto
    {
        public int IdFlujoEstadoTicket { get; set; }
        public int IdEstadoTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool EsEstadoFinal { get; set; }
        public int Orden { get; set; }
        public bool RequiereComentario { get; set; }
    }
}
