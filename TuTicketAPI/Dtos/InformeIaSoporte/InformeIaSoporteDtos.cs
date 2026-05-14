namespace TuTicketAPI.Dtos.InformeIaSoporte
{
    public class InformeIaSoporteMensualDto
    {
        public ModeloIaDto ModeloIa { get; set; } = new();
        public PeriodoInformeDto Periodo { get; set; } = new();
        public ResumenInformeSoporteDto Resumen { get; set; } = new();
        public SlaInformeSoporteDto Sla { get; set; } = new();
        public List<ConteoInformeDto> TicketsPorEstado { get; set; } = [];
        public List<ConteoInformeDto> TicketsPorPrioridad { get; set; } = [];
        public List<ConteoInformeDto> CategoriasConMayorVolumen { get; set; } = [];
        public List<ConteoInformeDto> SubcategoriasMasProblematicas { get; set; } = [];
        public List<ConteoInformeDto> ModulosAfectados { get; set; } = [];
        public List<ConteoInformeDto> UsuariosConMasSolicitudes { get; set; } = [];
        public List<ConteoInformeDto> ResponsablesConMasTickets { get; set; } = [];
        public List<TerminoFrecuenteInformeDto> ProblemasRecurrentes { get; set; } = [];
        public List<TerminoFrecuenteInformeDto> ComentariosFrecuentes { get; set; } = [];
        public List<TicketMuestraInformeDto> TicketsMuestra { get; set; } = [];
        public List<string> InstruccionesAnalisis { get; set; } = [];
        public string PromptSistema { get; set; } = string.Empty;
        public string PromptUsuario { get; set; } = string.Empty;
        public GeminiPayloadDto PayloadGemini { get; set; } = new();
    }

    public class ModeloIaDto
    {
        public string Nombre { get; set; } = "gemini-3.1-flash-lite";
        public string Proveedor { get; set; } = "Google";
    }

    public class PeriodoInformeDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
    }

    public class ResumenInformeSoporteDto
    {
        public int TicketsCreados { get; set; }
        public int TicketsAbiertos { get; set; }
        public int TicketsCerrados { get; set; }
        public int TicketsPendientes { get; set; }
        public int TicketsResueltos { get; set; }
        public int TicketsReabiertos { get; set; }
        public int TicketsSinAsignar { get; set; }
        public DuracionPromedioDto TiempoPromedioPrimeraRespuesta { get; set; } = new();
        public DuracionPromedioDto TiempoPromedioResolucion { get; set; } = new();
        public string? CategoriaMasCritica { get; set; }
        public string? SubcategoriaMasCritica { get; set; }
        public string? ProblemaMasRepetido { get; set; }
    }

    public class SlaInformeSoporteDto
    {
        public int TotalSlas { get; set; }
        public int TicketsConSlaVencido { get; set; }
        public int PrimeraRespuestaVencida { get; set; }
        public int ResolucionVencida { get; set; }
        public int EnRiesgo24Horas { get; set; }
        public decimal CumplimientoPorcentaje { get; set; }
    }

    public class DuracionPromedioDto
    {
        public decimal? Minutos { get; set; }
        public string Texto { get; set; } = "Sin datos";
    }

    public class ConteoInformeDto
    {
        public string Etiqueta { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class TerminoFrecuenteInformeDto
    {
        public string Texto { get; set; } = string.Empty;
        public int Frecuencia { get; set; }
    }

    public class TicketMuestraInformeDto
    {
        public int IdTicket { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Subcategoria { get; set; } = string.Empty;
        public string Solicitante { get; set; } = string.Empty;
        public string Responsable { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaPrimeraRespuesta { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public bool TieneSlaVencido { get; set; }
        public List<string> Comentarios { get; set; } = [];
        public List<string> Historial { get; set; } = [];
    }

    public class GeminiPayloadDto
    {
        public string Model { get; set; } = "gemini-3.1-flash-lite";
        public string Provider { get; set; } = "Google";
        public List<GeminiContentDto> Contents { get; set; } = [];
        public GeminiGenerationConfigDto GenerationConfig { get; set; } = new();
    }

    public class GeminiContentDto
    {
        public string Role { get; set; } = "user";
        public List<GeminiPartDto> Parts { get; set; } = [];
    }

    public class GeminiPartDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiGenerationConfigDto
    {
        public decimal Temperature { get; set; } = 0.2m;
        public int MaxOutputTokens { get; set; } = 4096;
    }

    public class InformeIaGeneradoDto
    {
        public InformeIaSoporteMensualDto Contexto { get; set; } = new();
        public string Contenido { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public string? ModeloVersion { get; set; }
        public string? ResponseId { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public GeminiUsageMetadataDto? Uso { get; set; }
    }

    public class GeminiGenerateContentResponseDto
    {
        public List<GeminiCandidateDto> Candidates { get; set; } = [];
        public GeminiUsageMetadataDto? UsageMetadata { get; set; }
        public string? ModelVersion { get; set; }
        public string? ResponseId { get; set; }
    }

    public class GeminiCandidateDto
    {
        public GeminiContentDto Content { get; set; } = new();
        public string? FinishReason { get; set; }
        public int Index { get; set; }
    }

    public class GeminiUsageMetadataDto
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }

    public class GeminiListModelsResponseDto
    {
        public List<GeminiModelDto> Models { get; set; } = [];
        public string? NextPageToken { get; set; }
    }

    public class GeminiModelDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public List<string> SupportedGenerationMethods { get; set; } = [];
    }
}
