using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Models;

namespace TuTicketAPI.Services.Tickets
{
    public class TicketHistoryService : ITicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public TicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public TicketHistorial CrearHistorial(int idTicket, string campo, string? valorAnterior, string? valorNuevo, string idUsuarioModificacion, string? comentario)
        {
            return new TicketHistorial
            {
                IdTicket = idTicket,
                CampoModificado = campo,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                IdUsuarioModificacion = idUsuarioModificacion,
                Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
            };
        }

        public void RegistrarCambio(DbSet<TicketHistorial> historiales, int idTicket, string campo, string? valorAnterior, string? valorNuevo, string idUsuarioModificacion, string? comentario)
        {
            if (valorAnterior == valorNuevo)
            {
                return;
            }

            historiales.Add(CrearHistorial(idTicket, campo, valorAnterior, valorNuevo, idUsuarioModificacion, comentario));
        }

        public async Task<string> ObtenerNombreUsuario(string idUsuario)
        {
            return await _context.Users
                .Where(u => u.Id == idUsuario)
                .Select(u => u.NombreCompleto)
                .FirstOrDefaultAsync() ?? idUsuario;
        }

        public async Task<string> ObtenerNombreUsuarioOpcional(string? idUsuario)
        {
            return string.IsNullOrWhiteSpace(idUsuario)
                ? "Sin usuario asignado"
                : await ObtenerNombreUsuario(idUsuario);
        }

        public async Task<string> ObtenerNombrePrioridad(int idPrioridadTicket)
        {
            return await _context.PrioridadTickets
                .Where(p => p.IdPrioridadTicket == idPrioridadTicket)
                .Select(p => p.Nombre)
                .FirstOrDefaultAsync() ?? idPrioridadTicket.ToString();
        }

        public async Task<string> ObtenerNombreEstado(int idEstadoTicket)
        {
            return await _context.EstadoTickets
                .Where(e => e.IdEstadoTicket == idEstadoTicket)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync() ?? idEstadoTicket.ToString();
        }

        public async Task<string> ObtenerNombreSubcategoria(int idSubcategoriaTicket)
        {
            var subcategoria = await _context.SubcategoriaTickets
                .Where(s => s.IdSubcategoriaTicket == idSubcategoriaTicket)
                .Select(s => new
                {
                    Categoria = s.CategoriaTicket.Nombre,
                    Subcategoria = s.Nombre
                })
                .FirstOrDefaultAsync();

            return subcategoria is null
                ? idSubcategoriaTicket.ToString()
                : $"{subcategoria.Categoria} / {subcategoria.Subcategoria}";
        }

        public string ConstruirComentarioCambio(string nombreUsuarioModificacion, string accion, string? valorAnterior, string? valorNuevo, string? comentarioUsuario)
        {
            var comentario = $"{nombreUsuarioModificacion} {accion}. Valor anterior: {FormatearValorHistorial(valorAnterior)}. Nuevo valor: {FormatearValorHistorial(valorNuevo)}.";

            if (!string.IsNullOrWhiteSpace(comentarioUsuario))
            {
                comentario = $"{comentario} Comentario: {comentarioUsuario.Trim()}";
            }

            return comentario;
        }

        private static string FormatearValorHistorial(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? "Sin valor" : valor;
        }
    }
}
