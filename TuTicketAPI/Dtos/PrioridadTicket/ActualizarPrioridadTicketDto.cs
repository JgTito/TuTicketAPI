using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.PrioridadTicket
{
    public class ActualizarPrioridadTicketDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue)]
        public int Nivel { get; set; }

        public bool Activo { get; set; } = true;
    }
}
