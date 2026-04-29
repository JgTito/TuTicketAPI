namespace TuTicketAPI.Dtos.PrioridadTicket
{
    public class PrioridadTicketSelectDto
    {
        public int IdPrioridadTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Nivel { get; set; }
    }
}
