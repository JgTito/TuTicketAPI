using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.TicketBitacora
{
    public class ActualizarTicketBitacoraDto
    {
        [Required]
        public string Comentario { get; set; } = string.Empty;

        public bool EsInterno { get; set; }
        public bool Activo { get; set; } = true;
    }
}
