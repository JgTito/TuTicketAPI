namespace TuTicketAPI.Dtos.Graficos
{
    public class GraficoSerieTemporalDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Etiqueta { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
