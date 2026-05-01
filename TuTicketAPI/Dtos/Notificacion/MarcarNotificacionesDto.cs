using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Notificacion
{
    public class MarcarNotificacionesDto
    {
        [Required]
        [MinLength(1)]
        public List<int> IdNotificaciones { get; set; } = new();
    }
}
