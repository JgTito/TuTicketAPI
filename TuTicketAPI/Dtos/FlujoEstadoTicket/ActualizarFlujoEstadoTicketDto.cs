using System.ComponentModel.DataAnnotations;

namespace TuTicketAPI.Dtos.FlujoEstadoTicket
{
    public class ActualizarFlujoEstadoTicketDto
    {
        [Range(1, int.MaxValue)]
        public int IdEstadoOrigen { get; set; }

        [Range(1, int.MaxValue)]
        public int IdEstadoDestino { get; set; }

        public bool RequiereComentario { get; set; }

        public bool Activo { get; set; } = true;
    }
}
