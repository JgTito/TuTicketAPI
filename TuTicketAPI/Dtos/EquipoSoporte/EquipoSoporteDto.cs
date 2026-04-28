namespace TuTicketAPI.Dtos.EquipoSoporte
{
    public class EquipoSoporteDto
    {
        public int IdEquipoSoporte { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
