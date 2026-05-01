using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Ticket
{
    public class ActualizarTicketDto
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

        public string? IdUsuarioAsignado { get; set; }

        [Required]
        public string IdUsuarioModificacion { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Comentario { get; set; }
    }
}
