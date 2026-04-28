namespace TuTicketAPI.Models
{
    public class CategoriaEquipoSoporte
    {
        public int IdCategoriaEquipoSoporte { get; set; }
        public int IdCategoriaTicket { get; set; }
        public int IdEquipoSoporte { get; set; }
        public bool Activo { get; set; } = true;

        public CategoriaTicket CategoriaTicket { get; set; } = null!;
        public EquipoSoporte EquipoSoporte { get; set; } = null!;
    }
}
