namespace TuTicketAPI.Dtos.SubcategoriaTicket
{
    public class SubcategoriaTicketSelectDto
    {
        public int IdSubcategoriaTicket { get; set; }
        public int IdCategoriaTicket { get; set; }
        public string NombreCategoriaTicket { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}
