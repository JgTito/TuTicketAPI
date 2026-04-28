using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.TicketRelacion
{
    public class CrearTicketRelacionDto
    {
        [Range(1, int.MaxValue)]
        public int IdTicketRelacionado { get; set; }

        [Range(1, int.MaxValue)]
        public int IdTipoRelacionTicket { get; set; }

        [MaxLength(500)]
        public string? Observacion { get; set; }

        [Required]
        public string IdUsuarioCreacion { get; set; } = string.Empty;
    }
}
