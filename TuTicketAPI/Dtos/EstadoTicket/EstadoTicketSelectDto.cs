namespace TuTicketAPI.Dtos.EstadoTicket
{
    public class EstadoTicketSelectDto
    {
        public int IdEstadoTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EsEstadoFinal { get; set; }
        public int Orden { get; set; }
    }
}
