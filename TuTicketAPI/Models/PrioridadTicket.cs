namespace TuTicketAPI.Models
{
    public class PrioridadTicket
    {
        public int IdPrioridadTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Nivel { get; set; }
        public bool Activo { get; set; } = true;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<SlaRegla> SlaReglas { get; set; } = new List<SlaRegla>();
    }
}
