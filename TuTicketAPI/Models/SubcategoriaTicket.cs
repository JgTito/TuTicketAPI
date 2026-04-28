namespace TuTicketAPI.Models
{
    public class SubcategoriaTicket
    {
        public int IdSubcategoriaTicket { get; set; }
        public int IdCategoriaTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        public CategoriaTicket CategoriaTicket { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
