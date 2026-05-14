using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Usuario
{
    public class ResetPasswordUsuarioDto
    {
        [Required]
        [MinLength(6)]
        public string NuevaPassword { get; set; } = string.Empty;
    }
}
