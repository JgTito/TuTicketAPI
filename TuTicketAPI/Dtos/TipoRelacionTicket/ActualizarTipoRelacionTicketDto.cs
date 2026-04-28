using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.TipoRelacionTicket
{
    public class ActualizarTipoRelacionTicketDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
