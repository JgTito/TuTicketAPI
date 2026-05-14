namespace TuTicketAPI.Dtos.Ticket
{
    public class TicketSelectDto
    {
        public int IdTicket { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string NombreEstadoTicket { get; set; } = string.Empty;
    }
}
