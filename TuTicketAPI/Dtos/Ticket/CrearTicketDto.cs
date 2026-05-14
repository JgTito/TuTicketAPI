using System.ComponentModel.DataAnnotations;
using TuTicketAPI.Dtos.TicketRelacion;

namespace TuTicketAPI.Dtos.Ticket
{
    public class CrearTicketDto
    {
        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int IdPrioridadTicket { get; set; }

        [Range(1, int.MaxValue)]
        public int IdSubcategoriaTicket { get; set; }

        public List<CrearTicketRelacionDto>? Relaciones { get; set; }

        public List<IFormFile>? Archivos { get; set; }
    }
}
