namespace TuTicketAPI.Dtos.SlaPolitica
{
    public class SlaPoliticaDto
    {
        public int IdSlaPolitica { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
