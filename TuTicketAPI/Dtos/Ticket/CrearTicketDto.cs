using System.ComponentModel.DataAnnotations;

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
        public int IdEstadoTicket { get; set; }

        [Range(1, int.MaxValue)]
        public int IdPrioridadTicket { get; set; }

        [Range(1, int.MaxValue)]
        public int IdSubcategoriaTicket { get; set; }

    }
}
