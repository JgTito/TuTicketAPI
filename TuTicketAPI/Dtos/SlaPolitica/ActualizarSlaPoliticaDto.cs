using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.SlaPolitica
{
    public class ActualizarSlaPoliticaDto
    {
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
