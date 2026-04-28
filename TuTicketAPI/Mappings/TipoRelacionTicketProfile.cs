using AutoMapper;
using TuTicketAPI.Dtos.TipoRelacionTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class TipoRelacionTicketProfile : Profile
    {
        public TipoRelacionTicketProfile()
        {
            CreateMap<TipoRelacionTicket, TipoRelacionTicketDto>();
            CreateMap<CrearTipoRelacionTicketDto, TipoRelacionTicket>();
            CreateMap<ActualizarTipoRelacionTicketDto, TipoRelacionTicket>();
        }
    }
}
