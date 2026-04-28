namespace TuTicketAPI.Models
{
    public class FlujoEstadoTicket
    {
        public int IdFlujoEstadoTicket { get; set; }
        public int IdEstadoOrigen { get; set; }
        public int IdEstadoDestino { get; set; }
        public bool RequiereComentario { get; set; }
        public bool Activo { get; set; } = true;

        public EstadoTicket EstadoOrigen { get; set; } = null!;
        public EstadoTicket EstadoDestino { get; set; } = null!;
    }
}
