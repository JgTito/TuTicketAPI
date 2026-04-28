namespace TuTicketAPI.Dtos.EstadoTicket
{
    public class EstadoTicketDto
    {
        public int IdEstadoTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool EsEstadoFinal { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }
}
