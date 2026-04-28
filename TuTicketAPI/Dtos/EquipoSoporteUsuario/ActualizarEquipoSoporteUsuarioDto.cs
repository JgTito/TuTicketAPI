using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.EquipoSoporteUsuario
{
    public class ActualizarEquipoSoporteUsuarioDto
    {
        [Range(1, int.MaxValue)]
        public int IdEquipoSoporte { get; set; }

        [Required]
        public string IdUsuario { get; set; } = string.Empty;

        public bool EsLider { get; set; }
        public bool Activo { get; set; } = true;
    }
}
