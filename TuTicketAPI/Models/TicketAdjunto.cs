namespace TuTicketAPI.Models
{
    public class TicketAdjunto
    {
        public int IdTicketAdjunto { get; set; }
        public int IdTicket { get; set; }
        public string NombreArchivoOriginal { get; set; } = string.Empty;
        public string NombreArchivoGuardado { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public string? TipoContenido { get; set; }
        public string? Extension { get; set; }
        public long? PesoBytes { get; set; }
        public string IdUsuarioSubida { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public bool Activo { get; set; } = true;

        public Ticket Ticket { get; set; } = null!;
        public ApplicationUser UsuarioSubida { get; set; } = null!;
    }
}
