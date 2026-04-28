namespace TuTicketAPI.Dtos.TicketBitacora
{
    public class TicketBitacoraDto
    {
        public int IdTicketBitacora { get; set; }
        public int IdTicket { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public bool EsInterno { get; set; }
        public string IdUsuarioCreacion { get; set; } = string.Empty;
        public string NombreUsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
