using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.CategoriaResponsable
{
    public class CrearCategoriaResponsableDto
    {
        [Range(1, int.MaxValue)]
        public int IdCategoriaTicket { get; set; }

        [Required]
        public string IdUsuarioResponsable { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
