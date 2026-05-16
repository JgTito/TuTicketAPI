using System.Text;
using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TuTicketAPI.Dtos.InformeIaSoporte;

namespace TuTicketAPI.Services.Informes
{
    public class InformeIaPdfService : IInformeIaPdfService
    {
        private const string ColorPrimario = "#0F4C81";
        private const string ColorAcento = "#0E7490";
        private const string ColorTexto = "#1F2937";
        private const string ColorTextoSuave = "#6B7280";
        private const string ColorBorde = "#D1D5DB";
        private const string ColorFondo = "#F3F7FA";

        public byte[] GenerarPdf(InformeIaGeneradoDto informe)
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(34);
                    page.DefaultTextStyle(text => text.FontSize(10).FontColor(ColorTexto));
                    page.Header().Element(container => ComponerHeader(container, informe));
                    page.Content().PaddingVertical(16).Column(column =>
                    {
                        column.Spacing(14);
                        ComponerResumenKpi(column, informe);
                        ComponerDatosClave(column, informe);
                        ComponerInformeIa(column, informe.Contenido);
                    });
                    page.Footer().Element(ComponerFooter);
                });
            }).GeneratePdf();
        }

        private static void ComponerHeader(IContainer container, InformeIaGeneradoDto informe)
        {
            var periodo = informe.Contexto.Periodo;

            container.BorderBottom(1).BorderColor(ColorBorde).PaddingBottom(12).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Informe mensual inteligente de soporte")
                        .FontSize(20)
                        .SemiBold()
                        .FontColor(ColorPrimario);

                    column.Item().PaddingTop(3).Text($"{Capitalizar(periodo.NombreMes)} {periodo.Anio}")
                        .FontSize(12)
                        .FontColor(ColorTextoSuave);
                });

                row.ConstantItem(145).AlignRight().Column(column =>
                {
                    column.Item().AlignRight().Text("TuTicket")
                        .FontSize(15)
                        .SemiBold()
                        .FontColor(ColorAcento);

                    column.Item().AlignRight().PaddingTop(3).Text("Analisis IA de soporte")
                        .FontSize(9)
                        .FontColor(ColorTextoSuave);
                });
            });
        }

        private static void ComponerResumenKpi(ColumnDescriptor column, InformeIaGeneradoDto informe)
        {
            var resumen = informe.Contexto.Resumen;
            var sla = informe.Contexto.Sla;

            column.Item().Row(row =>
            {
                row.Spacing(8);
                row.RelativeItem().Element(container => ComponerKpi(container, "Tickets creados", resumen.TicketsCreados.ToString(), "Total del periodo"));
                row.RelativeItem().Element(container => ComponerKpi(container, "Cerrados", resumen.TicketsCerrados.ToString(), "Estados finales"));
                row.RelativeItem().Element(container => ComponerKpi(container, "Pendientes", resumen.TicketsPendientes.ToString(), "En gestion"));
                row.RelativeItem().Element(container => ComponerKpi(container, "Cumplimiento SLA", $"{sla.CumplimientoPorcentaje:0.##}%", "Reglas cumplidas"));
            });

            column.Item().Row(row =>
            {
                row.Spacing(8);
                row.RelativeItem().Element(container => ComponerKpi(container, "Primera respuesta", resumen.TiempoPromedioPrimeraRespuesta.Texto, "Promedio"));
                row.RelativeItem().Element(container => ComponerKpi(container, "Resolucion", resumen.TiempoPromedioResolucion.Texto, "Promedio"));
                row.RelativeItem().Element(container => ComponerKpi(container, "Categoria critica", resumen.CategoriaMasCritica ?? "Sin datos", "Mayor volumen"));
                row.RelativeItem().Element(container => ComponerKpi(container, "Problema repetido", resumen.ProblemaMasRepetido ?? "Sin datos", "Termino frecuente"));
            });
        }

        private static void ComponerKpi(IContainer container, string titulo, string valor, string detalle)
        {
            container.Border(1).BorderColor(ColorBorde).Background(ColorFondo).Padding(10).MinHeight(72).Column(column =>
            {
                column.Item().Text(titulo.ToUpperInvariant())
                    .FontSize(7)
                    .SemiBold()
                    .FontColor(ColorTextoSuave);

                column.Item().PaddingTop(5).Text(valor)
                    .FontSize(13)
                    .SemiBold()
                    .FontColor(ColorPrimario);

                column.Item().PaddingTop(3).Text(detalle)
                    .FontSize(8)
                    .FontColor(ColorTextoSuave);
            });
        }

        private static void ComponerDatosClave(ColumnDescriptor column, InformeIaGeneradoDto informe)
        {
            column.Item().Row(row =>
            {
                row.Spacing(10);
                row.RelativeItem().Element(container => ComponerListaConteo(
                    container,
                    "Categorias con mayor volumen",
                    informe.Contexto.CategoriasConMayorVolumen));

                row.RelativeItem().Element(container => ComponerListaConteo(
                    container,
                    "Subcategorias problematicas",
                    informe.Contexto.SubcategoriasMasProblematicas));

                row.RelativeItem().Element(container => ComponerListaTerminos(
                    container,
                    "Problemas recurrentes",
                    informe.Contexto.ProblemasRecurrentes));
            });
        }

        private static void ComponerListaConteo(IContainer container, string titulo, IEnumerable<ConteoInformeDto> datos)
        {
            container.Border(1).BorderColor(ColorBorde).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text(titulo).FontSize(11).SemiBold().FontColor(ColorPrimario);

                foreach (var item in datos.Take(5))
                {
                    column.Item().Text($"{item.Etiqueta}: {item.Cantidad} ({item.Porcentaje:0.##}%)")
                        .FontSize(8)
                        .FontColor(ColorTexto);
                }
            });
        }

        private static void ComponerListaTerminos(IContainer container, string titulo, IEnumerable<TerminoFrecuenteInformeDto> datos)
        {
            container.Border(1).BorderColor(ColorBorde).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text(titulo).FontSize(11).SemiBold().FontColor(ColorPrimario);

                foreach (var item in datos.Take(5))
                {
                    column.Item().Text($"{item.Texto}: {item.Frecuencia}")
                        .FontSize(8)
                        .FontColor(ColorTexto);
                }
            });
        }

        private static void ComponerInformeIa(ColumnDescriptor column, string contenido)
        {
            column.Item().PaddingTop(2).Text("Resumen ejecutivo generado por IA")
                .FontSize(14)
                .SemiBold()
                .FontColor(ColorPrimario);

            var lineas = contenido.Replace("\r\n", "\n").Split('\n');
            var parrafo = new StringBuilder();

            foreach (var lineaOriginal in lineas)
            {
                var linea = lineaOriginal.Trim();

                if (string.IsNullOrWhiteSpace(linea))
                {
                    RenderizarParrafoPendiente(column, parrafo);
                    column.Item().Height(4);
                    continue;
                }

                if (EsTitulo(linea) || EsBullet(linea) || EsNumeracion(linea))
                {
                    RenderizarParrafoPendiente(column, parrafo);
                    RenderizarLineaEspecial(column, linea);
                    continue;
                }

                if (parrafo.Length > 0)
                {
                    parrafo.Append(' ');
                }

                parrafo.Append(LimpiarMarkdown(linea));
            }

            RenderizarParrafoPendiente(column, parrafo);
        }

        private static void RenderizarLineaEspecial(ColumnDescriptor column, string linea)
        {
            if (linea.StartsWith("### ", StringComparison.Ordinal))
            {
                RenderizarTitulo(column, linea[4..], 11);
                return;
            }

            if (linea.StartsWith("## ", StringComparison.Ordinal))
            {
                RenderizarTitulo(column, linea[3..], 12);
                return;
            }

            if (linea.StartsWith("# ", StringComparison.Ordinal))
            {
                RenderizarTitulo(column, linea[2..], 13);
                return;
            }

            if (EsBullet(linea))
            {
                var texto = linea[2..];
                column.Item().Row(row =>
                {
                    row.ConstantItem(12).Text("-").FontColor(ColorAcento).SemiBold();
                    row.RelativeItem().Text(LimpiarMarkdown(texto)).FontSize(10).LineHeight(1.25f);
                });
                return;
            }

            var match = Regex.Match(linea, @"^(\d+)\.\s+(.*)$");
            if (match.Success)
            {
                column.Item().Row(row =>
                {
                    row.ConstantItem(20).Text($"{match.Groups[1].Value}.").FontColor(ColorAcento).SemiBold();
                    row.RelativeItem().Text(LimpiarMarkdown(match.Groups[2].Value)).FontSize(10).LineHeight(1.25f);
                });
            }
        }

        private static void RenderizarTitulo(ColumnDescriptor column, string texto, int fontSize)
        {
            column.Item().PaddingTop(7).PaddingBottom(2).Text(LimpiarMarkdown(texto))
                .FontSize(fontSize)
                .SemiBold()
                .FontColor(ColorPrimario);
        }

        private static void RenderizarParrafoPendiente(ColumnDescriptor column, StringBuilder parrafo)
        {
            if (parrafo.Length == 0)
            {
                return;
            }

            column.Item().Text(parrafo.ToString())
                .FontSize(10)
                .LineHeight(1.25f)
                .FontColor(ColorTexto);

            parrafo.Clear();
        }

        private static void ComponerFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(ColorBorde).PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text("TuTicket - Informe generado automaticamente con IA")
                    .FontSize(8)
                    .FontColor(ColorTextoSuave);

                row.ConstantItem(90).AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(8).FontColor(ColorTextoSuave));
                    text.Span("Pagina ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }

        private static bool EsTitulo(string linea)
        {
            return linea.StartsWith("# ", StringComparison.Ordinal) ||
                   linea.StartsWith("## ", StringComparison.Ordinal) ||
                   linea.StartsWith("### ", StringComparison.Ordinal);
        }

        private static bool EsBullet(string linea)
        {
            return linea.StartsWith("- ", StringComparison.Ordinal) ||
                   linea.StartsWith("* ", StringComparison.Ordinal);
        }

        private static bool EsNumeracion(string linea)
        {
            return Regex.IsMatch(linea, @"^\d+\.\s+");
        }

        private static string LimpiarMarkdown(string texto)
        {
            return texto
                .Replace("**", string.Empty, StringComparison.Ordinal)
                .Replace("__", string.Empty, StringComparison.Ordinal)
                .Replace("`", string.Empty, StringComparison.Ordinal);
        }

        private static string Capitalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return texto;
            }

            return char.ToUpperInvariant(texto[0]) + texto[1..];
        }
    }
}
