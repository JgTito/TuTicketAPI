using AutoMapper;
using TuTicketAPI.Dtos.TicketSla;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class TicketSlaProfile : Profile
    {
        public TicketSlaProfile()
        {
            CreateMap<TicketSla, TicketSlaDto>()
                .ForMember(dest => dest.CodigoTicket, opt => opt.MapFrom(src => src.Ticket.Codigo))
                .ForMember(dest => dest.NombreSlaPolitica, opt => opt.MapFrom(src => src.SlaRegla.SlaPolitica.Nombre))
                .ForMember(dest => dest.NombrePrioridadTicket, opt => opt.MapFrom(src => src.SlaRegla.PrioridadTicket.Nombre))
                .ForMember(dest => dest.NombreCategoriaTicket, opt => opt.MapFrom(src => src.SlaRegla.CategoriaTicket == null ? null : src.SlaRegla.CategoriaTicket.Nombre));
        }
    }
}
