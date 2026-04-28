namespace TuTicketAPI.Dtos.FlujoEstadoTicket
{
    public class FlujoEstadoTicketDto
    {
        public int IdFlujoEstadoTicket { get; set; }
        public int IdEstadoOrigen { get; set; }
        public string NombreEstadoOrigen { get; set; } = string.Empty;
        public int IdEstadoDestino { get; set; }
        public string NombreEstadoDestino { get; set; } = string.Empty;
        public bool RequiereComentario { get; set; }
        public bool Activo { get; set; }
    }
}
