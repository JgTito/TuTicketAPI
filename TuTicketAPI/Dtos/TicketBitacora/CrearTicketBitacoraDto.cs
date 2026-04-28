using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.TicketBitacora
{
    public class CrearTicketBitacoraDto
    {
        [Required]
        public string Comentario { get; set; } = string.Empty;

        public bool EsInterno { get; set; }

        [Required]
        public string IdUsuarioCreacion { get; set; } = string.Empty;
    }
}
