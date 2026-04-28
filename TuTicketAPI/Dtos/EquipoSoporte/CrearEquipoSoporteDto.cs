using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.EquipoSoporte
{
    public class CrearEquipoSoporteDto
    {
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
