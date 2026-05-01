using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Models;

namespace TuTicketAPI.Services.Tickets
{
    public class TicketNotificationService : ITicketNotificationService
    {
        private readonly ApplicationDbContext _context;

        public TicketNotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task NotificarCreacionTicket(Ticket ticket, string idUsuarioActor)
        {
            await NotificarUsuario(
                ticket.IdUsuarioSolicitante,
                ticket.IdTicket,
                $"Ticket {ticket.Codigo} creado",
                $"Tu ticket {ticket.Codigo} fue creado y enviado al responsable de categoria.");

            await NotificarUsuario(
                ticket.IdUsuarioAsignado,
                ticket.IdTicket,
                $"Nuevo ticket {ticket.Codigo}",
                $"Se te asigno automaticamente el ticket {ticket.Codigo}: {ticket.Titulo}.");
        }

        public Task NotificarActualizacionTicket(Ticket ticket, string idUsuarioActor, string? comentario)
        {
            var mensaje = $"El ticket {ticket.Codigo} fue actualizado.";
            mensaje = AgregarComentario(mensaje, comentario);

            return NotificarParticipantes(ticket, idUsuarioActor, $"Ticket {ticket.Codigo} actualizado", mensaje);
        }

        public async Task NotificarAsignacionTicket(Ticket ticket, string idUsuarioActor, string idUsuarioAsignado, string? comentario)
        {
            var mensajeAsignado = $"Se te asigno el ticket {ticket.Codigo}: {ticket.Titulo}.";
            await NotificarUsuario(idUsuarioAsignado, ticket.IdTicket, $"Ticket {ticket.Codigo} asignado", AgregarComentario(mensajeAsignado, comentario), idUsuarioActor);

            var mensajeParticipantes = $"El ticket {ticket.Codigo} fue asignado a un responsable.";
            await NotificarParticipantes(ticket, idUsuarioActor, $"Asignacion del ticket {ticket.Codigo}", AgregarComentario(mensajeParticipantes, comentario), idUsuarioAsignado);
        }

        public Task NotificarCambioEstadoTicket(Ticket ticket, string idUsuarioActor, string estadoAnterior, string estadoNuevo, string? comentario)
        {
            var mensaje = $"El ticket {ticket.Codigo} cambio de estado desde {estadoAnterior} a {estadoNuevo}.";
            mensaje = AgregarComentario(mensaje, comentario);

            return NotificarParticipantes(ticket, idUsuarioActor, $"Cambio de estado {ticket.Codigo}", mensaje);
        }

        public Task NotificarCambioPrioridadTicket(Ticket ticket, string idUsuarioActor, string prioridadAnterior, string prioridadNueva, string? comentario)
        {
            var mensaje = $"El ticket {ticket.Codigo} cambio de prioridad desde {prioridadAnterior} a {prioridadNueva}.";
            mensaje = AgregarComentario(mensaje, comentario);

            return NotificarParticipantes(ticket, idUsuarioActor, $"Cambio de prioridad {ticket.Codigo}", mensaje);
        }

        public async Task NotificarComentarioTicket(int idTicket, string idUsuarioActor, bool esInterno)
        {
            var ticket = await ObtenerTicket(idTicket);

            if (ticket is null)
            {
                return;
            }

            var titulo = $"Nuevo comentario en {ticket.Codigo}";
            var mensaje = esInterno
                ? $"Se agrego un comentario interno al ticket {ticket.Codigo}."
                : $"Se agrego un comentario al ticket {ticket.Codigo}.";

            if (esInterno)
            {
                await NotificarUsuario(ticket.IdUsuarioAsignado, ticket.IdTicket, titulo, mensaje, idUsuarioActor);
                return;
            }

            await NotificarParticipantes(ticket, idUsuarioActor, titulo, mensaje);
        }

        public async Task NotificarAdjuntosTicket(int idTicket, string idUsuarioActor, int cantidadAdjuntos)
        {
            var ticket = await ObtenerTicket(idTicket);

            if (ticket is null || cantidadAdjuntos < 1)
            {
                return;
            }

            var plural = cantidadAdjuntos == 1 ? "adjunto" : "adjuntos";
            await NotificarParticipantes(
                ticket,
                idUsuarioActor,
                $"Nuevos adjuntos en {ticket.Codigo}",
                $"Se agregaron {cantidadAdjuntos} {plural} al ticket {ticket.Codigo}.");
        }

        private async Task NotificarParticipantes(
            Ticket ticket,
            string idUsuarioActor,
            string titulo,
            string mensaje,
            string? idUsuarioExcluidoAdicional = null)
        {
            var destinatarios = new HashSet<string>();

            AgregarDestinatario(destinatarios, ticket.IdUsuarioSolicitante, idUsuarioActor, idUsuarioExcluidoAdicional);
            AgregarDestinatario(destinatarios, ticket.IdUsuarioAsignado, idUsuarioActor, idUsuarioExcluidoAdicional);

            foreach (var idUsuario in destinatarios)
            {
                await NotificarUsuario(idUsuario, ticket.IdTicket, titulo, mensaje);
            }
        }

        private async Task NotificarUsuario(string? idUsuarioDestino, int idTicket, string titulo, string mensaje, string? idUsuarioExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(idUsuarioDestino) || idUsuarioDestino == idUsuarioExcluir)
            {
                return;
            }

            var usuarioActivo = await _context.Users.AnyAsync(u => u.Id == idUsuarioDestino && u.Activo);

            if (!usuarioActivo)
            {
                return;
            }

            _context.Notificaciones.Add(new Notificacion
            {
                IdUsuarioDestino = idUsuarioDestino,
                IdTicket = idTicket,
                Titulo = titulo,
                Mensaje = mensaje,
                Leida = false,
                FechaCreacion = DateTime.Now
            });
        }

        private Task<Ticket?> ObtenerTicket(int idTicket)
        {
            return _context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTicket == idTicket);
        }

        private static void AgregarDestinatario(HashSet<string> destinatarios, string? idUsuario, string idUsuarioActor, string? idUsuarioExcluidoAdicional)
        {
            if (!string.IsNullOrWhiteSpace(idUsuario) &&
                idUsuario != idUsuarioActor &&
                idUsuario != idUsuarioExcluidoAdicional)
            {
                destinatarios.Add(idUsuario);
            }
        }

        private static string AgregarComentario(string mensaje, string? comentario)
        {
            return string.IsNullOrWhiteSpace(comentario)
                ? mensaje
                : $"{mensaje} Comentario: {comentario}";
        }
    }
}
