using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Ticket
{
    public class CambiarPrioridadTicketDto
    {
        [Range(1, int.MaxValue)]
        public int IdPrioridadTicket { get; set; }

        [Required]
        public string IdUsuarioModificacion { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Comentario { get; set; }
    }
}
