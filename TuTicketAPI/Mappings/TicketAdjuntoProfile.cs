using AutoMapper;
using TuTicketAPI.Dtos.TicketAdjunto;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class TicketAdjuntoProfile : Profile
    {
        public TicketAdjuntoProfile()
        {
            CreateMap<TicketAdjunto, TicketAdjuntoDto>()
                .ForMember(dest => dest.CodigoTicket, opt => opt.MapFrom(src => src.Ticket.Codigo))
                .ForMember(dest => dest.NombreUsuarioSubida, opt => opt.MapFrom(src => src.UsuarioSubida.NombreCompleto));
        }
    }
}
