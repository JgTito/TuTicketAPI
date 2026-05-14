using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Usuario
{
    public class ActualizarAdminUsuarioDto
    {
        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
