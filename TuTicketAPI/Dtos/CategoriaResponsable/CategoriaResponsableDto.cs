namespace TuTicketAPI.Dtos.CategoriaResponsable
{
    public class CategoriaResponsableDto
    {
        public int IdCategoriaResponsable { get; set; }
        public int IdCategoriaTicket { get; set; }
        public string NombreCategoriaTicket { get; set; } = string.Empty;
        public string IdUsuarioResponsable { get; set; } = string.Empty;
        public string NombreUsuarioResponsable { get; set; } = string.Empty;
        public string? EmailUsuarioResponsable { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
