using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.SubcategoriaTicket
{
    public class ActualizarSubcategoriaTicketDto
    {
        [Range(1, int.MaxValue)]
        public int IdCategoriaTicket { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
