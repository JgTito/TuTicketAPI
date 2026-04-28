using AutoMapper;
using TuTicketAPI.Dtos.TicketRelacion;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class TicketRelacionProfile : Profile
    {
        public TicketRelacionProfile()
        {
            CreateMap<TicketRelacion, TicketRelacionDto>()
                .ForMember(dest => dest.CodigoTicketOrigen, opt => opt.MapFrom(src => src.TicketOrigen.Codigo))
                .ForMember(dest => dest.CodigoTicketRelacionado, opt => opt.MapFrom(src => src.TicketRelacionado.Codigo))
                .ForMember(dest => dest.NombreTipoRelacionTicket, opt => opt.MapFrom(src => src.TipoRelacionTicket.Nombre))
                .ForMember(dest => dest.NombreUsuarioCreacion, opt => opt.MapFrom(src => src.UsuarioCreacion.NombreCompleto));
            CreateMap<CrearTicketRelacionDto, TicketRelacion>()
                .ForMember(dest => dest.IdTicketOrigen, opt => opt.Ignore());
            CreateMap<ActualizarTicketRelacionDto, TicketRelacion>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.IdUsuarioCreacion, opt => opt.Ignore());
        }
    }
}
