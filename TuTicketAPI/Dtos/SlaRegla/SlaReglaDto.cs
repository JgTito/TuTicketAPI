namespace TuTicketAPI.Dtos.SlaRegla
{
    public class SlaReglaDto
    {
        public int IdSlaRegla { get; set; }
        public int IdSlaPolitica { get; set; }
        public string NombreSlaPolitica { get; set; } = string.Empty;
        public int IdPrioridadTicket { get; set; }
        public string NombrePrioridadTicket { get; set; } = string.Empty;
        public int? IdCategoriaTicket { get; set; }
        public string? NombreCategoriaTicket { get; set; }
        public int MinutosPrimeraRespuesta { get; set; }
        public int MinutosResolucion { get; set; }
        public bool Activo { get; set; }
    }
}
