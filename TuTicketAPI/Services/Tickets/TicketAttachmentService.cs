using TuTicketAPI.Models;

namespace TuTicketAPI.Services.Tickets
{
    public class TicketAttachmentService : ITicketAttachmentService
    {
        private readonly IWebHostEnvironment _environment;

        public TicketAttachmentService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IReadOnlyList<string> ValidarArchivos(IReadOnlyList<IFormFile>? archivos, bool requiereAlMenosUno)
        {
            var errores = new List<string>();

            if (archivos is null || archivos.Count == 0)
            {
                if (requiereAlMenosUno)
                {
                    errores.Add("Debe adjuntar al menos un archivo.");
                }

                return errores;
            }

            for (var i = 0; i < archivos.Count; i++)
            {
                if (archivos[i].Length == 0)
                {
                    errores.Add($"El archivo en la posicion {i + 1} esta vacio.");
                }
            }

            return errores;
        }

        public async Task<List<TicketAdjunto>> GuardarAdjuntos(int idTicket, IEnumerable<IFormFile>? archivos, string idUsuarioSubida, ICollection<string> rutasGuardadas)
        {
            var adjuntos = new List<TicketAdjunto>();

            if (archivos is null)
            {
                return adjuntos;
            }

            var directorioTicket = Path.Combine(_environment.ContentRootPath, "Uploads", "Tickets", idTicket.ToString());
            Directory.CreateDirectory(directorioTicket);

            foreach (var archivo in archivos)
            {
                var extension = Path.GetExtension(archivo.FileName);
                var nombreGuardado = $"{Guid.NewGuid():N}{extension}";
                var rutaFisica = Path.Combine(directorioTicket, nombreGuardado);

                await using (var stream = File.Create(rutaFisica))
                {
                    await archivo.CopyToAsync(stream);
                }

                rutasGuardadas.Add(rutaFisica);

                adjuntos.Add(new TicketAdjunto
                {
                    IdTicket = idTicket,
                    NombreArchivoOriginal = Path.GetFileName(archivo.FileName),
                    NombreArchivoGuardado = nombreGuardado,
                    RutaArchivo = rutaFisica,
                    TipoContenido = string.IsNullOrWhiteSpace(archivo.ContentType) ? "application/octet-stream" : archivo.ContentType,
                    Extension = string.IsNullOrWhiteSpace(extension) ? null : extension,
                    PesoBytes = archivo.Length,
                    IdUsuarioSubida = idUsuarioSubida,
                    Activo = true
                });
            }

            return adjuntos;
        }

        public void EliminarArchivosGuardados(IEnumerable<string> rutasGuardadas)
        {
            foreach (var ruta in rutasGuardadas)
            {
                try
                {
                    if (File.Exists(ruta))
                    {
                        File.Delete(ruta);
                    }
                }
                catch
                {
                    // La limpieza de archivos no debe ocultar el error original.
                }
            }
        }
    }
}
