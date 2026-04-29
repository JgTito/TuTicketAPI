using Microsoft.AspNetCore.Mvc;
using TuTicketAPI.Dtos.Comun;

namespace TuTicketAPI.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected ActionResult? ValidarPaginacion(int pagina, int tamanoPagina)
        {
            if (pagina < 1)
            {
                ModelState.AddModelError(nameof(pagina), "La pagina debe ser mayor o igual a 1.");
            }

            if (tamanoPagina < 1 || tamanoPagina > 100)
            {
                ModelState.AddModelError(nameof(tamanoPagina), "El tamano de pagina debe estar entre 1 y 100.");
            }

            return ModelState.IsValid ? null : ValidationProblem(ModelState);
        }

        protected static ResultadoPaginadoDto<TDto> CrearResultadoPaginado<TDto>(
            int pagina,
            int tamanoPagina,
            int totalRegistros,
            IEnumerable<TDto> datos)
        {
            return new ResultadoPaginadoDto<TDto>
            {
                Pagina = pagina,
                TamanoPagina = tamanoPagina,
                TotalRegistros = totalRegistros,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina),
                Datos = datos
            };
        }
    }
}
