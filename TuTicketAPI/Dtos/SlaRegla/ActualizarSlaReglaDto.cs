using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.SlaRegla
{
    public class ActualizarSlaReglaDto
    {
        [Range(1, int.MaxValue)]
        public int IdSlaPolitica { get; set; }

        [Range(1, int.MaxValue)]
        public int IdPrioridadTicket { get; set; }

        public int? IdCategoriaTicket { get; set; }

        [Range(1, int.MaxValue)]
        public int MinutosPrimeraRespuesta { get; set; }

        [Range(1, int.MaxValue)]
        public int MinutosResolucion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
