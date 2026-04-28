namespace TuTicketAPI.Dtos.SubcategoriaTicket
{
    public class SubcategoriaTicketDto
    {
        public int IdSubcategoriaTicket { get; set; }
        public int IdCategoriaTicket { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
