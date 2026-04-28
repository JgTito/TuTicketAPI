namespace TuTicketAPI.Dtos.CategoriaTicket
{
    public class CategoriaTicketDto
    {
        public int IdCategoriaTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
