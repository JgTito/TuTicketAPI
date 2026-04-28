namespace TuTicketAPI.Models
{
    public class CategoriaTicket
    {
        public int IdCategoriaTicket { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        public ICollection<SubcategoriaTicket> Subcategorias { get; set; } = new List<SubcategoriaTicket>();
        public ICollection<CategoriaResponsable> Responsables { get; set; } = new List<CategoriaResponsable>();
        public ICollection<CategoriaEquipoSoporte> EquiposSoporte { get; set; } = new List<CategoriaEquipoSoporte>();
        public ICollection<SlaRegla> SlaReglas { get; set; } = new List<SlaRegla>();
    }
}
