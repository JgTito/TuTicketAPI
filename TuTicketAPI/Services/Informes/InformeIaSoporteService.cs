using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TuTicketAPI.Dtos.InformeIaSoporte;
using TuTicketAPI.Models;
using TuTicketAPI.Services.Tickets;

namespace TuTicketAPI.Services.Informes
{
    public class InformeIaSoporteService : IInformeIaSoporteService
    {
        private const string ModeloIa = "gemini-3.1-flash-lite";
        private const string ProveedorIa = "Google";
        private static readonly CultureInfo CulturaEs = new("es-CL");

        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "para", "como", "este", "esta", "estos", "estas", "desde", "hasta", "ticket", "tickets",
            "problema", "error", "solicitud", "usuario", "usuarios", "sistema", "caso", "casos",
            "favor", "revisar", "revision", "crear", "actualizar", "cambio", "datos", "dato",
            "tiene", "tener", "puede", "porque", "cuando", "donde", "sobre", "todo", "todos",
            "segun", "interno", "externo", "nuevo", "nueva", "mismo", "misma", "sin", "con"
        };

        private readonly ApplicationDbContext _context;
        private readonly ITicketAccessService _ticketAccessService;

        public InformeIaSoporteService(ApplicationDbContext context, ITicketAccessService ticketAccessService)
        {
            _context = context;
            _ticketAccessService = ticketAccessService;
        }

        public async Task<InformeIaSoporteMensualDto> CrearContextoMensualAsync(
            int? anio = null,
            int? mes = null,
            int limiteTicketsMuestra = 40,
            bool aplicarFiltroAcceso = true,
            CancellationToken cancellationToken = default)
        {
            var ahora = DateTime.Now;
            var anioInforme = anio ?? ahora.Year;
            var mesInforme = mes ?? ahora.Month;

            ValidarParametros(anioInforme, mesInforme, limiteTicketsMuestra);

            var fechaDesde = new DateTime(anioInforme, mesInforme, 1);
            var fechaHastaExclusiva = fechaDesde.AddMonths(1);
            var fechaHasta = fechaHastaExclusiva.AddTicks(-1);

            var tickets = await CrearTicketQuery(fechaDesde, fechaHastaExclusiva, aplicarFiltroAcceso)
                .OrderByDescending(t => t.FechaCreacion)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            var informe = CrearInforme(tickets, anioInforme, mesInforme, fechaDesde, fechaHasta, limiteTicketsMuestra);

            informe.PromptSistema = CrearPromptSistema();
            informe.PromptUsuario = CrearPromptUsuario(informe);
            informe.PayloadGemini = CrearPayloadGemini(informe.PromptSistema, informe.PromptUsuario);

            return informe;
        }

        private IQueryable<Ticket> CrearTicketQuery(DateTime fechaDesde, DateTime fechaHastaExclusiva, bool aplicarFiltroAcceso)
        {
            var query = _context.Tickets
                .Include(t => t.EstadoTicket)
                .Include(t => t.PrioridadTicket)
                .Include(t => t.SubcategoriaTicket)
                    .ThenInclude(s => s.CategoriaTicket)
                .Include(t => t.UsuarioSolicitante)
                .Include(t => t.UsuarioAsignado)
                .Include(t => t.Slas.Where(s => s.Activo))
                .Include(t => t.Bitacoras.Where(b => b.Activo))
                    .ThenInclude(b => b.UsuarioCreacion)
                .Include(t => t.Historiales)
                    .ThenInclude(h => h.UsuarioModificacion)
                .AsNoTracking()
                .Where(t => t.FechaCreacion >= fechaDesde && t.FechaCreacion < fechaHastaExclusiva);

            return aplicarFiltroAcceso ? _ticketAccessService.AplicarFiltroAcceso(query) : query;
        }

        private static InformeIaSoporteMensualDto CrearInforme(
            IReadOnlyList<Ticket> tickets,
            int anio,
            int mes,
            DateTime fechaDesde,
            DateTime fechaHasta,
            int limiteTicketsMuestra)
        {
            var totalTickets = tickets.Count;
            var ticketsConSlaVencido = tickets.Count(t => t.Slas.Any(SlaVencido));
            var primeraRespuestaVencida = tickets.Sum(t => t.Slas.Count(s => s.PrimeraRespuestaVencida));
            var resolucionVencida = tickets.Sum(t => t.Slas.Count(s => s.ResolucionVencida));
            var totalSlas = tickets.Sum(t => t.Slas.Count);
            var slasCumplidos = tickets.Sum(t => t.Slas.Count(s => !s.PrimeraRespuestaVencida && !s.ResolucionVencida));

            var categorias = CrearConteos(tickets, t => t.SubcategoriaTicket.CategoriaTicket.Nombre, totalTickets, 10);
            var subcategorias = CrearConteos(tickets, t => t.SubcategoriaTicket.Nombre, totalTickets, 10);
            var problemasRecurrentes = ExtraerTerminosFrecuentes(
                tickets.SelectMany(t => new[] { t.Titulo, t.Descripcion }),
                15);
            var comentariosFrecuentes = ExtraerTerminosFrecuentes(
                tickets.SelectMany(t => t.Bitacoras.Select(b => b.Comentario)),
                15);

            return new InformeIaSoporteMensualDto
            {
                ModeloIa = new ModeloIaDto
                {
                    Nombre = ModeloIa,
                    Proveedor = ProveedorIa
                },
                Periodo = new PeriodoInformeDto
                {
                    Anio = anio,
                    Mes = mes,
                    NombreMes = CulturaEs.DateTimeFormat.GetMonthName(mes),
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta
                },
                Resumen = new ResumenInformeSoporteDto
                {
                    TicketsCreados = totalTickets,
                    TicketsAbiertos = tickets.Count(t => !t.EstadoTicket.EsEstadoFinal),
                    TicketsCerrados = tickets.Count(t => t.EstadoTicket.EsEstadoFinal),
                    TicketsPendientes = tickets.Count(t => !t.EstadoTicket.EsEstadoFinal),
                    TicketsResueltos = tickets.Count(t =>
                        t.FechaResolucion.HasValue ||
                        t.FechaCierre.HasValue ||
                        t.EstadoTicket.Nombre.Contains("Resuelto", StringComparison.OrdinalIgnoreCase) ||
                        t.EstadoTicket.Nombre.Contains("Cerrado", StringComparison.OrdinalIgnoreCase)),
                    TicketsReabiertos = tickets.Count(t => t.CantidadReaperturas > 0),
                    TicketsSinAsignar = tickets.Count(t => string.IsNullOrWhiteSpace(t.IdUsuarioAsignado)),
                    TiempoPromedioPrimeraRespuesta = CalcularPromedio(tickets
                        .Where(t => t.FechaPrimeraRespuesta.HasValue)
                        .Select(t => t.FechaPrimeraRespuesta!.Value - t.FechaCreacion)),
                    TiempoPromedioResolucion = CalcularPromedio(tickets
                        .Where(t => t.FechaResolucion.HasValue || t.FechaCierre.HasValue)
                        .Select(t => (t.FechaResolucion ?? t.FechaCierre)!.Value - t.FechaCreacion)),
                    CategoriaMasCritica = categorias.FirstOrDefault()?.Etiqueta,
                    SubcategoriaMasCritica = subcategorias.FirstOrDefault()?.Etiqueta,
                    ProblemaMasRepetido = problemasRecurrentes.FirstOrDefault()?.Texto
                },
                Sla = new SlaInformeSoporteDto
                {
                    TotalSlas = totalSlas,
                    TicketsConSlaVencido = ticketsConSlaVencido,
                    PrimeraRespuestaVencida = primeraRespuestaVencida,
                    ResolucionVencida = resolucionVencida,
                    EnRiesgo24Horas = tickets.Count(t => t.Slas.Any(SlaEnRiesgo24Horas)),
                    CumplimientoPorcentaje = totalSlas == 0 ? 0 : Math.Round(slasCumplidos * 100m / totalSlas, 2)
                },
                TicketsPorEstado = CrearConteos(tickets, t => t.EstadoTicket.Nombre, totalTickets, 20),
                TicketsPorPrioridad = CrearConteos(tickets, t => t.PrioridadTicket.Nombre, totalTickets, 10),
                CategoriasConMayorVolumen = categorias,
                SubcategoriasMasProblematicas = subcategorias,
                ModulosAfectados = CrearConteos(tickets, t => $"{t.SubcategoriaTicket.CategoriaTicket.Nombre} / {t.SubcategoriaTicket.Nombre}", totalTickets, 10),
                UsuariosConMasSolicitudes = CrearConteos(tickets, t => t.UsuarioSolicitante.NombreCompleto, totalTickets, 10),
                ResponsablesConMasTickets = CrearConteos(tickets, t => t.UsuarioAsignado?.NombreCompleto ?? "Sin responsable", totalTickets, 10),
                ProblemasRecurrentes = problemasRecurrentes,
                ComentariosFrecuentes = comentariosFrecuentes,
                TicketsMuestra = CrearTicketsMuestra(tickets, limiteTicketsMuestra),
                InstruccionesAnalisis = CrearInstruccionesAnalisis()
            };
        }

        private static List<TicketMuestraInformeDto> CrearTicketsMuestra(IReadOnlyList<Ticket> tickets, int limite)
        {
            return tickets
                .OrderByDescending(t => t.Slas.Any(SlaVencido))
                .ThenByDescending(t => t.FechaCreacion)
                .Take(limite)
                .Select(t => new TicketMuestraInformeDto
                {
                    IdTicket = t.IdTicket,
                    Codigo = t.Codigo,
                    Titulo = Limitar(t.Titulo, 180),
                    Descripcion = Limitar(t.Descripcion, 600),
                    Estado = t.EstadoTicket.Nombre,
                    Prioridad = t.PrioridadTicket.Nombre,
                    Categoria = t.SubcategoriaTicket.CategoriaTicket.Nombre,
                    Subcategoria = t.SubcategoriaTicket.Nombre,
                    Solicitante = t.UsuarioSolicitante.NombreCompleto,
                    Responsable = t.UsuarioAsignado?.NombreCompleto ?? "Sin responsable",
                    FechaCreacion = t.FechaCreacion,
                    FechaPrimeraRespuesta = t.FechaPrimeraRespuesta,
                    FechaResolucion = t.FechaResolucion,
                    FechaCierre = t.FechaCierre,
                    TieneSlaVencido = t.Slas.Any(SlaVencido),
                    Comentarios = t.Bitacoras
                        .OrderByDescending(b => b.FechaCreacion)
                        .Take(3)
                        .Select(b => Limitar(b.Comentario, 300))
                        .ToList(),
                    Historial = t.Historiales
                        .OrderByDescending(h => h.FechaModificacion)
                        .Take(3)
                        .Select(h => Limitar(h.Comentario ?? $"{h.CampoModificado}: {h.ValorAnterior} -> {h.ValorNuevo}", 300))
                        .ToList()
                })
                .ToList();
        }

        private static List<ConteoInformeDto> CrearConteos(
            IEnumerable<Ticket> tickets,
            Func<Ticket, string> selector,
            int total,
            int limite)
        {
            return tickets
                .GroupBy(selector)
                .Select(g => new ConteoInformeDto
                {
                    Etiqueta = string.IsNullOrWhiteSpace(g.Key) ? "Sin dato" : g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = total == 0 ? 0 : Math.Round(g.Count() * 100m / total, 2)
                })
                .OrderByDescending(x => x.Cantidad)
                .ThenBy(x => x.Etiqueta)
                .Take(limite)
                .ToList();
        }

        private static List<TerminoFrecuenteInformeDto> ExtraerTerminosFrecuentes(IEnumerable<string?> textos, int limite)
        {
            var conteos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var texto in textos.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var normalizado = QuitarAcentos(texto!.ToLowerInvariant());
                var limpio = Regex.Replace(normalizado, @"[^a-z0-9\s]", " ");
                var palabras = limpio
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(p => p.Length >= 4 && !StopWords.Contains(p));

                foreach (var palabra in palabras)
                {
                    conteos[palabra] = conteos.GetValueOrDefault(palabra) + 1;
                }
            }

            return conteos
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Take(limite)
                .Select(kv => new TerminoFrecuenteInformeDto
                {
                    Texto = kv.Key,
                    Frecuencia = kv.Value
                })
                .ToList();
        }

        private static DuracionPromedioDto CalcularPromedio(IEnumerable<TimeSpan> duraciones)
        {
            var lista = duraciones.Where(d => d.TotalMinutes >= 0).ToList();

            if (lista.Count == 0)
            {
                return new DuracionPromedioDto();
            }

            var minutos = (decimal)lista.Average(d => d.TotalMinutes);

            return new DuracionPromedioDto
            {
                Minutos = Math.Round(minutos, 2),
                Texto = FormatearDuracion(minutos)
            };
        }

        private static bool SlaVencido(TicketSla sla)
        {
            return sla.PrimeraRespuestaVencida || sla.ResolucionVencida;
        }

        private static bool SlaEnRiesgo24Horas(TicketSla sla)
        {
            var ahora = DateTime.Now;
            var limite = ahora.AddHours(24);

            return (sla.FechaPrimeraRespuestaReal is null &&
                    sla.FechaLimitePrimeraRespuesta >= ahora &&
                    sla.FechaLimitePrimeraRespuesta <= limite) ||
                   (sla.FechaResolucionReal is null &&
                    sla.FechaLimiteResolucion >= ahora &&
                    sla.FechaLimiteResolucion <= limite);
        }

        private static string CrearPromptSistema()
        {
            return """
Eres un analista senior de soporte TI. Debes generar informes ejecutivos mensuales para jefaturas y lideres tecnicos.
Usa solo los datos entregados por el sistema. No inventes cifras, tickets, categorias ni causas.
Tu salida debe estar en espanol, ser clara, profesional, accionable y orientada a decisiones.
Cuando una causa raiz sea inferida, indicalo como inferencia y explica la evidencia usada.
""";
        }

        private static string CrearPromptUsuario(InformeIaSoporteMensualDto informe)
        {
            var contexto = new
            {
                informe.Periodo,
                informe.Resumen,
                informe.Sla,
                informe.TicketsPorEstado,
                informe.TicketsPorPrioridad,
                informe.CategoriasConMayorVolumen,
                informe.SubcategoriasMasProblematicas,
                informe.ModulosAfectados,
                informe.UsuariosConMasSolicitudes,
                informe.ResponsablesConMasTickets,
                informe.ProblemasRecurrentes,
                informe.ComentariosFrecuentes,
                informe.TicketsMuestra
            };

            var json = JsonSerializer.Serialize(contexto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            return $"""
Genera un informe mensual inteligente de soporte para el periodo {informe.Periodo.NombreMes} {informe.Periodo.Anio}.

Objetivo:
- Crear un resumen ejecutivo para administradores, jefaturas y lideres tecnicos.
- Detectar problemas recurrentes, modulos afectados, riesgos SLA y posibles causas raiz.
- Proponer recomendaciones concretas y priorizadas.

Formato esperado:
1. Titulo del informe.
2. Resumen ejecutivo en 2 a 4 parrafos.
3. Indicadores principales en bullets.
4. Hallazgos relevantes.
5. Problemas recurrentes y evidencia.
6. Riesgos o alertas operativas.
7. Recomendaciones accionables numeradas.
8. Supuestos o limitaciones del analisis.

Datos disponibles en JSON:
{json}
""";
        }

        private static GeminiPayloadDto CrearPayloadGemini(string promptSistema, string promptUsuario)
        {
            return new GeminiPayloadDto
            {
                Model = ModeloIa,
                Provider = ProveedorIa,
                Contents =
                [
                    new GeminiContentDto
                    {
                        Role = "user",
                        Parts =
                        [
                            new GeminiPartDto
                            {
                                Text = promptSistema + Environment.NewLine + Environment.NewLine + promptUsuario
                            }
                        ]
                    }
                ],
                GenerationConfig = new GeminiGenerationConfigDto
                {
                    Temperature = 0.2m,
                    MaxOutputTokens = 4096
                }
            };
        }

        private static List<string> CrearInstruccionesAnalisis()
        {
            return
            [
                "Analizar cantidad total de tickets, abiertos, cerrados, pendientes y resueltos.",
                "Identificar categorias, subcategorias o modulos con mayor volumen.",
                "Detectar problemas recurrentes a partir de titulos, descripciones y comentarios.",
                "Evaluar cumplimiento SLA, tickets vencidos y tickets en riesgo.",
                "Calcular impacto operacional usando volumen, prioridad y vencimientos SLA.",
                "Inferir posibles causas raiz solo cuando exista evidencia suficiente.",
                "Generar recomendaciones concretas, priorizadas y accionables."
            ];
        }

        private static void ValidarParametros(int anio, int mes, int limiteTicketsMuestra)
        {
            if (anio < 2000 || anio > 2100)
            {
                throw new ArgumentOutOfRangeException(nameof(anio), "El anio debe estar entre 2000 y 2100.");
            }

            if (mes < 1 || mes > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(mes), "El mes debe estar entre 1 y 12.");
            }

            if (limiteTicketsMuestra < 1 || limiteTicketsMuestra > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(limiteTicketsMuestra), "El limite de tickets de muestra debe estar entre 1 y 100.");
            }
        }

        private static string FormatearDuracion(decimal minutos)
        {
            var totalMinutos = (int)Math.Round(minutos, 0);
            var horas = totalMinutos / 60;
            var minutosRestantes = totalMinutos % 60;

            if (horas <= 0)
            {
                return $"{minutosRestantes}m";
            }

            return $"{horas}h {minutosRestantes}m";
        }

        private static string Limitar(string texto, int maximo)
        {
            if (texto.Length <= maximo)
            {
                return texto;
            }

            return texto[..maximo] + "...";
        }

        private static string QuitarAcentos(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var caracter in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);

                if (categoria != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(caracter);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
