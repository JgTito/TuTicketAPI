using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.TicketRelacion
{
    public class ActualizarTicketRelacionDto
    {
        [Range(1, int.MaxValue)]
        public int IdTicketOrigen { get; set; }

        [Range(1, int.MaxValue)]
        public int IdTicketRelacionado { get; set; }

        [Range(1, int.MaxValue)]
        public int IdTipoRelacionTicket { get; set; }

        [MaxLength(500)]
        public string? Observacion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
