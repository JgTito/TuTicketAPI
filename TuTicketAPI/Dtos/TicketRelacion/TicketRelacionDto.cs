namespace TuTicketAPI.Dtos.TicketRelacion
{
    public class TicketRelacionDto
    {
        public int IdTicketRelacion { get; set; }
        public int IdTicketOrigen { get; set; }
        public string CodigoTicketOrigen { get; set; } = string.Empty;
        public int IdTicketRelacionado { get; set; }
        public string CodigoTicketRelacionado { get; set; } = string.Empty;
        public int IdTipoRelacionTicket { get; set; }
        public string NombreTipoRelacionTicket { get; set; } = string.Empty;
        public string? Observacion { get; set; }
        public string IdUsuarioCreacion { get; set; } = string.Empty;
        public string NombreUsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
