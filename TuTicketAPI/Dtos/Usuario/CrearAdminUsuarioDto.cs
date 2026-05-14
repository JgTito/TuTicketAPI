using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Usuario
{
    public class CrearAdminUsuarioDto
    {
        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        [Required]
        [MinLength(1)]
        public List<string> Roles { get; set; } = new();
    }
}
