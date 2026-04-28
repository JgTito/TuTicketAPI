using AutoMapper;
using TuTicketAPI.Dtos.TicketBitacora;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class TicketBitacoraProfile : Profile
    {
        public TicketBitacoraProfile()
        {
            CreateMap<TicketBitacora, TicketBitacoraDto>()
                .ForMember(dest => dest.NombreUsuarioCreacion, opt => opt.MapFrom(src => src.UsuarioCreacion.NombreCompleto));
            CreateMap<CrearTicketBitacoraDto, TicketBitacora>();
            CreateMap<ActualizarTicketBitacoraDto, TicketBitacora>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.IdUsuarioCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.IdTicket, opt => opt.Ignore());
        }
    }
}
