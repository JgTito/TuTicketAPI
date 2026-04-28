namespace TuTicketAPI.Models
{
    public class EquipoSoporteUsuario
    {
        public int IdEquipoSoporteUsuario { get; set; }
        public int IdEquipoSoporte { get; set; }
        public string IdUsuario { get; set; } = string.Empty;
        public bool EsLider { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        public EquipoSoporte EquipoSoporte { get; set; } = null!;
        public ApplicationUser Usuario { get; set; } = null!;
    }
}
