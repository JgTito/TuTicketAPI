using AutoMapper;
using TuTicketAPI.Dtos.SlaRegla;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class SlaReglaProfile : Profile
    {
        public SlaReglaProfile()
        {
            CreateMap<SlaRegla, SlaReglaDto>()
                .ForMember(dest => dest.NombreSlaPolitica, opt => opt.MapFrom(src => src.SlaPolitica.Nombre))
                .ForMember(dest => dest.NombrePrioridadTicket, opt => opt.MapFrom(src => src.PrioridadTicket.Nombre))
                .ForMember(dest => dest.NombreCategoriaTicket, opt => opt.MapFrom(src => src.CategoriaTicket == null ? null : src.CategoriaTicket.Nombre));
            CreateMap<CrearSlaReglaDto, SlaRegla>();
            CreateMap<ActualizarSlaReglaDto, SlaRegla>();
        }
    }
}
