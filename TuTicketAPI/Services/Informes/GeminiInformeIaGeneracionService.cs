using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TuTicketAPI.Dtos.InformeIaSoporte;

namespace TuTicketAPI.Services.Informes
{
    public class GeminiInformeIaGeneracionService : IInformeIaGeneracionService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly IInformeIaSoporteService _informeIaSoporteService;
        private readonly GoogleGeminiOptions _options;

        public GeminiInformeIaGeneracionService(
            HttpClient httpClient,
            IInformeIaSoporteService informeIaSoporteService,
            IOptions<GoogleGeminiOptions> options)
        {
            _httpClient = httpClient;
            _informeIaSoporteService = informeIaSoporteService;
            _options = options.Value;
        }

        public async Task<InformeIaGeneradoDto> GenerarInformeMensualAsync(
            int? anio = null,
            int? mes = null,
            int limiteTicketsMuestra = 40,
            bool aplicarFiltroAcceso = true,
            CancellationToken cancellationToken = default)
        {
            ValidarConfiguracion();

            var contexto = await _informeIaSoporteService.CrearContextoMensualAsync(
                anio,
                mes,
                limiteTicketsMuestra,
                aplicarFiltroAcceso,
                cancellationToken);

            contexto.ModeloIa.Nombre = _options.Modelo;
            contexto.ModeloIa.Proveedor = _options.Proveedor;
            contexto.PayloadGemini.Model = _options.Modelo;
            contexto.PayloadGemini.Provider = _options.Proveedor;
            contexto.PayloadGemini.GenerationConfig.Temperature = _options.Temperatura;
            contexto.PayloadGemini.GenerationConfig.MaxOutputTokens = _options.MaxOutputTokens;

            var response = await EnviarAGeminiAsync(contexto.PayloadGemini, cancellationToken);
            var contenido = ExtraerTexto(response);

            return new InformeIaGeneradoDto
            {
                Contexto = contexto,
                Contenido = contenido,
                Modelo = _options.Modelo,
                Proveedor = _options.Proveedor,
                ModeloVersion = response.ModelVersion,
                ResponseId = response.ResponseId,
                FechaGeneracion = DateTime.Now,
                Uso = response.UsageMetadata
            };
        }

        private async Task<GeminiGenerateContentResponseDto> EnviarAGeminiAsync(
            GeminiPayloadDto payload,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, CrearRutaGenerateContent());
            request.Headers.Add("x-goog-api-key", _options.ApiKey);
            request.Content = JsonContent.Create(
                new GeminiGenerateContentRequestDto
                {
                    Contents = payload.Contents,
                    GenerationConfig = payload.GenerationConfig
                },
                options: JsonOptions);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await CrearDetalleErrorGeminiAsync(responseBody, cancellationToken);
                throw new InvalidOperationException(
                    $"Gemini respondio con estado {(int)response.StatusCode}: {detalle}");
            }

            var geminiResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponseDto>(responseBody, JsonOptions);

            return geminiResponse ?? throw new InvalidOperationException("Gemini no devolvio una respuesta valida.");
        }

        private string CrearRutaGenerateContent()
        {
            return $"models/{Uri.EscapeDataString(_options.Modelo)}:generateContent";
        }

        private async Task<string> CrearDetalleErrorGeminiAsync(string responseBody, CancellationToken cancellationToken)
        {
            if (!EsErrorModeloNoDisponible(responseBody))
            {
                return Limitar(responseBody, 1000);
            }

            var modelos = await ObtenerModelosGenerateContentAsync(cancellationToken);

            if (modelos.Count == 0)
            {
                return Limitar(responseBody, 1000);
            }

            var sugerencias = modelos
                .Where(m => m.Contains("flash", StringComparison.OrdinalIgnoreCase))
                .Take(12)
                .ToList();

            if (sugerencias.Count == 0)
            {
                sugerencias = modelos.Take(12).ToList();
            }

            return
                $"El modelo configurado '{_options.Modelo}' no esta disponible para generateContent. " +
                $"Modelos disponibles sugeridos: {string.Join(", ", sugerencias)}. " +
                $"Respuesta original: {Limitar(responseBody, 500)}";
        }

        private async Task<List<string>> ObtenerModelosGenerateContentAsync(CancellationToken cancellationToken)
        {
            var modelos = new List<string>();
            string? pageToken = null;

            do
            {
                var ruta = string.IsNullOrWhiteSpace(pageToken)
                    ? "models?pageSize=1000"
                    : $"models?pageSize=1000&pageToken={Uri.EscapeDataString(pageToken)}";

                using var request = new HttpRequestMessage(HttpMethod.Get, ruta);
                request.Headers.Add("x-goog-api-key", _options.ApiKey);

                using var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return modelos;
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var listResponse = JsonSerializer.Deserialize<GeminiListModelsResponseDto>(responseBody, JsonOptions);

                if (listResponse is null)
                {
                    return modelos;
                }

                modelos.AddRange(listResponse.Models
                    .Where(m => m.SupportedGenerationMethods.Contains("generateContent"))
                    .Select(m => m.Name.Replace("models/", string.Empty, StringComparison.OrdinalIgnoreCase))
                    .Where(m => !string.IsNullOrWhiteSpace(m)));

                pageToken = listResponse.NextPageToken;
            }
            while (!string.IsNullOrWhiteSpace(pageToken));

            return modelos
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => m)
                .ToList();
        }

        private string ExtraerTexto(GeminiGenerateContentResponseDto response)
        {
            var texto = string.Join(
                Environment.NewLine,
                response.Candidates
                    .SelectMany(c => c.Content.Parts)
                    .Select(p => p.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t)));

            if (string.IsNullOrWhiteSpace(texto))
            {
                throw new InvalidOperationException("Gemini no devolvio contenido de texto para el informe.");
            }

            return texto.Trim();
        }

        private void ValidarConfiguracion()
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("GoogleGemini:ApiKey no esta configurado.");
            }

            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                throw new InvalidOperationException("GoogleGemini:BaseUrl no esta configurado.");
            }

            if (string.IsNullOrWhiteSpace(_options.Modelo))
            {
                throw new InvalidOperationException("GoogleGemini:Modelo no esta configurado.");
            }

            if (_options.MaxOutputTokens < 1)
            {
                throw new InvalidOperationException("GoogleGemini:MaxOutputTokens debe ser mayor a 0.");
            }
        }

        private static string Limitar(string texto, int maximo)
        {
            if (texto.Length <= maximo)
            {
                return texto;
            }

            return texto[..maximo] + "...";
        }

        private static bool EsErrorModeloNoDisponible(string responseBody)
        {
            return responseBody.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase) ||
                   responseBody.Contains("is not found", StringComparison.OrdinalIgnoreCase) ||
                   responseBody.Contains("not supported for generateContent", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class GeminiGenerateContentRequestDto
        {
            [JsonPropertyName("contents")]
            public List<GeminiContentDto> Contents { get; set; } = [];

            [JsonPropertyName("generationConfig")]
            public GeminiGenerationConfigDto GenerationConfig { get; set; } = new();
        }
    }
}
