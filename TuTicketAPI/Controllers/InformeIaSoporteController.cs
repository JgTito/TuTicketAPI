using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuTicketAPI.Authorization;
using TuTicketAPI.Dtos.InformeIaSoporte;
using TuTicketAPI.Services.Informes;

namespace TuTicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Administrador)]
    public class InformeIaSoporteController : ApiControllerBase
    {
        private readonly IInformeIaGeneracionService _informeIaGeneracionService;

        public InformeIaSoporteController(IInformeIaGeneracionService informeIaGeneracionService)
        {
            _informeIaGeneracionService = informeIaGeneracionService;
        }

        [HttpGet("mensual/descargar")]
        public async Task<IActionResult> DescargarInformeMensual(
            [FromQuery] int? anio = null,
            [FromQuery] int? mes = null,
            [FromQuery] int limiteTicketsMuestra = 40,
            [FromQuery] string formato = "markdown",
            CancellationToken cancellationToken = default)
        {
            if (!ValidarFormato(formato))
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var informe = await _informeIaGeneracionService.GenerarInformeMensualAsync(
                    anio,
                    mes,
                    limiteTicketsMuestra,
                    cancellationToken: cancellationToken);

                var extension = ObtenerExtension(formato);
                var contentType = ObtenerContentType(formato);
                var nombreArchivo = CrearNombreArchivo(informe, extension);
                var contenido = CrearContenidoDescarga(informe);
                var bytes = Encoding.UTF8.GetBytes(contenido);

                return File(bytes, contentType, nombreArchivo);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ModelState.AddModelError(ex.ParamName ?? "parametro", ex.Message);
                return ValidationProblem(ModelState);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(
                    title: "No fue posible generar el informe IA.",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status502BadGateway);
            }
        }

        private bool ValidarFormato(string formato)
        {
            if (string.Equals(formato, "markdown", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(formato, "md", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(formato, "txt", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            ModelState.AddModelError(nameof(formato), "El formato debe ser markdown, md o txt.");
            return false;
        }

        private static string CrearContenidoDescarga(InformeIaGeneradoDto informe)
        {
            var periodo = informe.Contexto.Periodo;
            var builder = new StringBuilder();

            builder.AppendLine($"# Informe mensual de soporte - {periodo.NombreMes} {periodo.Anio}");
            builder.AppendLine();
            builder.AppendLine($"Generado: {informe.FechaGeneracion:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Proveedor IA: {informe.Proveedor}");
            builder.AppendLine($"Modelo: {informe.Modelo}");

            if (!string.IsNullOrWhiteSpace(informe.ModeloVersion))
            {
                builder.AppendLine($"Version modelo: {informe.ModeloVersion}");
            }

            if (!string.IsNullOrWhiteSpace(informe.ResponseId))
            {
                builder.AppendLine($"ResponseId: {informe.ResponseId}");
            }

            builder.AppendLine();
            builder.AppendLine("---");
            builder.AppendLine();
            builder.AppendLine(informe.Contenido.Trim());

            return builder.ToString();
        }

        private static string CrearNombreArchivo(InformeIaGeneradoDto informe, string extension)
        {
            var periodo = informe.Contexto.Periodo;
            return $"informe-soporte-{periodo.Anio}-{periodo.Mes:00}.{extension}";
        }

        private static string ObtenerExtension(string formato)
        {
            return string.Equals(formato, "txt", StringComparison.OrdinalIgnoreCase) ? "txt" : "md";
        }

        private static string ObtenerContentType(string formato)
        {
            return string.Equals(formato, "txt", StringComparison.OrdinalIgnoreCase)
                ? "text/plain; charset=utf-8"
                : "text/markdown; charset=utf-8";
        }
    }
}
