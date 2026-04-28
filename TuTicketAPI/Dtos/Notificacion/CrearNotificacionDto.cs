using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Notificacion
{
    public class CrearNotificacionDto
    {
        [Required]
        public string IdUsuarioDestino { get; set; } = string.Empty;

        public int? IdTicket { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Mensaje { get; set; } = string.Empty;
    }
}
