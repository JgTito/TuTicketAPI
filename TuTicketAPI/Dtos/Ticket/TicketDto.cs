namespace TuTicketAPI.Dtos.Ticket
{
    public class TicketDto
    {
        public int IdTicket { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int IdEstadoTicket { get; set; }
        public string NombreEstadoTicket { get; set; } = string.Empty;
        public int IdPrioridadTicket { get; set; }
        public string NombrePrioridadTicket { get; set; } = string.Empty;
        public int IdSubcategoriaTicket { get; set; }
        public string NombreSubcategoriaTicket { get; set; } = string.Empty;
        public int IdCategoriaTicket { get; set; }
        public string NombreCategoriaTicket { get; set; } = string.Empty;
        public string IdUsuarioSolicitante { get; set; } = string.Empty;
        public string NombreUsuarioSolicitante { get; set; } = string.Empty;
        public string? IdUsuarioAsignado { get; set; }
        public string? NombreUsuarioAsignado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaPrimeraRespuesta { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int CantidadReaperturas { get; set; }
        public bool Activo { get; set; }
    }
}
