using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.TicketSla
{
    public class RegistrarTicketSlaFechaDto
    {
        public DateTime? Fecha { get; set; }

        [Required]
        public string IdUsuarioModificacion { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Comentario { get; set; }
    }
}
