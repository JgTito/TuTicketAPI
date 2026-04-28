namespace TuTicketAPI.Dtos.EquipoSoporteUsuario
{
    public class EquipoSoporteUsuarioDto
    {
        public int IdEquipoSoporteUsuario { get; set; }
        public int IdEquipoSoporte { get; set; }
        public string NombreEquipoSoporte { get; set; } = string.Empty;
        public string IdUsuario { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string? EmailUsuario { get; set; }
        public bool EsLider { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
