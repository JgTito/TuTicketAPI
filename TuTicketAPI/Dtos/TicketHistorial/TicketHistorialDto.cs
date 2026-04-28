namespace TuTicketAPI.Dtos.TicketHistorial
{
    public class TicketHistorialDto
    {
        public int IdTicketHistorial { get; set; }
        public int IdTicket { get; set; }
        public string CampoModificado { get; set; } = string.Empty;
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }
        public string? Comentario { get; set; }
        public string IdUsuarioModificacion { get; set; } = string.Empty;
        public string NombreUsuarioModificacion { get; set; } = string.Empty;
        public DateTime FechaModificacion { get; set; }
    }
}
