namespace TuTicketAPI.Dtos.Comun
{
    public class ResultadoPaginadoDto<T>
    {
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public IEnumerable<T> Datos { get; set; } = Array.Empty<T>();
    }
}
