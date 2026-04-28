namespace TuTicketAPI.Dtos.TicketAdjunto
{
    public class TicketAdjuntoDto
    {
        public int IdTicketAdjunto { get; set; }
        public int IdTicket { get; set; }
        public string CodigoTicket { get; set; } = string.Empty;
        public string NombreArchivoOriginal { get; set; } = string.Empty;
        public string NombreArchivoGuardado { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public string? TipoContenido { get; set; }
        public string? Extension { get; set; }
        public long? PesoBytes { get; set; }
        public string IdUsuarioSubida { get; set; } = string.Empty;
        public string NombreUsuarioSubida { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public bool Activo { get; set; }
    }
}
