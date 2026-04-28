namespace TuTicketAPI.Models
{
    public class SlaPolitica
    {
        public int IdSlaPolitica { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        public ICollection<SlaRegla> SlaReglas { get; set; } = new List<SlaRegla>();
    }
}
