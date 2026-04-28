using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Usuario
{
    public class LoginUsuarioDto
    {
        [Required]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
