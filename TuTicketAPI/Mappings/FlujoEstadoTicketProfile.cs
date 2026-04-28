using AutoMapper;
using TuTicketAPI.Dtos.FlujoEstadoTicket;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class FlujoEstadoTicketProfile : Profile
    {
        public FlujoEstadoTicketProfile()
        {
            CreateMap<FlujoEstadoTicket, FlujoEstadoTicketDto>()
                .ForMember(dest => dest.NombreEstadoOrigen, opt => opt.MapFrom(src => src.EstadoOrigen.Nombre))
                .ForMember(dest => dest.NombreEstadoDestino, opt => opt.MapFrom(src => src.EstadoDestino.Nombre));
            CreateMap<CrearFlujoEstadoTicketDto, FlujoEstadoTicket>();
            CreateMap<ActualizarFlujoEstadoTicketDto, FlujoEstadoTicket>();
        }
    }
}
