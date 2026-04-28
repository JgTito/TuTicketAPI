namespace TuTicketAPI.Dtos.PrioridadTicket
{
    public class PrioridadTicketDto
    {
        public int IdPrioridadTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Nivel { get; set; }
        public bool Activo { get; set; }
    }
}
