using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.TicketAdjunto
{
    public class CrearTicketAdjuntoDto
    {
        [Required]
        public IFormFile Archivo { get; set; } = null!;

        [Required]
        public string IdUsuarioSubida { get; set; } = string.Empty;
    }
}
