namespace TuTicketAPI.Models
{
    public class EquipoSoporte
    {
        public int IdEquipoSoporte { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        public ICollection<EquipoSoporteUsuario> Usuarios { get; set; } = new List<EquipoSoporteUsuario>();
        public ICollection<CategoriaEquipoSoporte> Categorias { get; set; } = new List<CategoriaEquipoSoporte>();
    }
}
