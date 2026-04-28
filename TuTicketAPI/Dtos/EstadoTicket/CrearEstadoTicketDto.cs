using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.EstadoTicket
{
    public class CrearEstadoTicketDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        public bool EsEstadoFinal { get; set; }

        [Range(1, int.MaxValue)]
        public int Orden { get; set; }

        public bool Activo { get; set; } = true;
    }
}
