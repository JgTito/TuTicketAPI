namespace TuTicketAPI.Dtos.CategoriaEquipoSoporte
{
    public class CategoriaEquipoSoporteDto
    {
        public int IdCategoriaEquipoSoporte { get; set; }
        public int IdCategoriaTicket { get; set; }
        public string NombreCategoriaTicket { get; set; } = string.Empty;
        public int IdEquipoSoporte { get; set; }
        public string NombreEquipoSoporte { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
