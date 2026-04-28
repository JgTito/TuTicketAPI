namespace TuTicketAPI.Models
{
    public class CategoriaResponsable
    {
        public int IdCategoriaResponsable { get; set; }
        public int IdCategoriaTicket { get; set; }
        public string IdUsuarioResponsable { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        public CategoriaTicket CategoriaTicket { get; set; } = null!;
        public ApplicationUser UsuarioResponsable { get; set; } = null!;
    }
}
