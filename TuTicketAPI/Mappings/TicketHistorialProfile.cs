using AutoMapper;
using TuTicketAPI.Dtos.TicketHistorial;
using TuTicketAPI.Models;

namespace TuTicketAPI.Mappings
{
    public class TicketHistorialProfile : Profile
    {
        public TicketHistorialProfile()
        {
            CreateMap<TicketHistorial, TicketHistorialDto>()
                .ForMember(dest => dest.NombreUsuarioModificacion, opt => opt.MapFrom(src => src.UsuarioModificacion.NombreCompleto));
        }
    }
}
