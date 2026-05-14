using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.Usuario
{
    public class ActualizarRolesUsuarioDto
    {
        [Required]
        [MinLength(1)]
        public List<string> Roles { get; set; } = new();
    }
}
