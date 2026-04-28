using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Ticket
{
    public class AsignarTicketDto
    {
        [Required]
        public string IdUsuarioAsignado { get; set; } = string.Empty;

        [Required]
        public string IdUsuarioModificacion { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Comentario { get; set; }
    }
}
