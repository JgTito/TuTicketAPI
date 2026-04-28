using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.CategoriaEquipoSoporte
{
    public class CrearCategoriaEquipoSoporteDto
    {
        [Range(1, int.MaxValue)]
        public int IdCategoriaTicket { get; set; }

        [Range(1, int.MaxValue)]
        public int IdEquipoSoporte { get; set; }

        public bool Activo { get; set; } = true;
    }
}
