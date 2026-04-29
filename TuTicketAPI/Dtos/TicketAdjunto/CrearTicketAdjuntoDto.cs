using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.TicketAdjunto
{
    public class CrearTicketAdjuntoDto
    {
        [Required]
        [MinLength(1)]
        public List<IFormFile> Archivos { get; set; } = [];

        [Required]
        public string IdUsuarioSubida { get; set; } = string.Empty;
    }
}
