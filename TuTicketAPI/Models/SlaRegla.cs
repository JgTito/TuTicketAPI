namespace TuTicketAPI.Models
{
    public class SlaRegla
    {
        public int IdSlaRegla { get; set; }
        public int IdSlaPolitica { get; set; }
        public int IdPrioridadTicket { get; set; }
        public int? IdCategoriaTicket { get; set; }
        public int MinutosPrimeraRespuesta { get; set; }
        public int MinutosResolucion { get; set; }
        public bool Activo { get; set; } = true;

        public SlaPolitica SlaPolitica { get; set; } = null!;
        public PrioridadTicket PrioridadTicket { get; set; } = null!;
        public CategoriaTicket? CategoriaTicket { get; set; }
        public ICollection<TicketSla> TicketSlas { get; set; } = new List<TicketSla>();
    }
}
