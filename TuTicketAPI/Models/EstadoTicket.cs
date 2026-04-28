namespace TuTicketAPI.Models
{
    public class EstadoTicket
    {
        public int IdEstadoTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool EsEstadoFinal { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; } = true;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<FlujoEstadoTicket> FlujosOrigen { get; set; } = new List<FlujoEstadoTicket>();
        public ICollection<FlujoEstadoTicket> FlujosDestino { get; set; } = new List<FlujoEstadoTicket>();
    }
}
